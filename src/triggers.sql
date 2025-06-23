create or replace function check_uniqueness_post_title()
    returns trigger
    language plpgsql
as
$$
begin
    if exists (select title from post where title = new.title and company_id = new.company_id and id != new.id) then
        raise exception 'Post title must be unique in a company';
    end if;
    return new;
end;
$$;

create trigger check_uniqueness_post_title_trigger
    before insert or update
    on post
    for each row
execute function check_uniqueness_post_title();

create or replace function check_uniqueness_position_title()
    returns trigger
    language plpgsql
as
$$
begin
    if exists (select title from position where title = new.title and company_id = new.company_id and id != new.id) then
        raise exception 'Position title must be unique in a company';
    end if;
    return new;
end;
$$;

create trigger check_uniqueness_position_title_trigger
    before insert or update
    on position
    for each row
execute function check_uniqueness_position_title();

create or replace function check_non_crossing_positions_history()
    returns trigger
    language plpgsql
as
$$
begin
    if exists (select start_date, end_date, position_id, employee_id
               from position_history
               where position_id = new.position_id
                 and start_date <= new.start_date
                 and employee_id != new.employee_id
                 and (end_date is null or end_date > new.start_date)) then
        raise exception 'Position almost occupied';
    end if;
    return new;
end;
$$;

-- drop trigger check_non_crossing_positions_trigger on position_history;

create trigger check_non_crossing_positions_trigger
    before insert
    on position_history
    for each row
execute function check_non_crossing_positions_history();

-- drop trigger reassignment_positions_trigger on position;

-- create or replace function reassignment_positions()
--     returns trigger
--     language plpgsql
-- as
-- $$
-- begin
--     set session_replication_role = replica;
--     update position set parent_id = old.parent_id where parent_id = old.id;
--     update position_reduced set parent_id = old.parent_id where parent_id = old.id;
--     set session_replication_role = origin;
--     return null;
-- end;
-- $$;

-- drop trigger if exists reassignment_positions_trigger on position;

-- create trigger reassignment_positions_trigger
--     before delete
--     on position
--     for each row
-- execute function reassignment_positions();

create or replace function protect_column_parent_id()
    returns trigger
    language plpgsql
as
$$
begin
    if old.parent_id is distinct from new.parent_id and
       (select count(*) from position where parent_id = old.id) > 0 then
        raise exception 'Parent id cannot be changed directly for employees with subordinates, use the special functions: change_parent_id_with_subordinates(uuid, uuid), change_parent_id_without_subordinates(uuid, uuid)';
    end if;
    return new;
end;
$$;

create trigger protect_column_parent_id_trigger
    before update
    on position
    for each row
execute function protect_column_parent_id();

create or replace function check_scores_frequency()
    returns trigger
    language plpgsql
as
$$
begin
    if new.created_at - (select min(abs(score_story.created_at - new.created_at))
                         from score_story
                         where employee_id = new.employee_id
                           and position_id = new.position_id
                           and created_at < new.created_at) < interval '1 month' then
        raise exception 'Scores frequency must be at least 1 month';
    end if;
    return new;
end;
$$;

create trigger check_scores_frequency_trigger
    before insert or update
    on score_story
    for each row
execute function check_scores_frequency();

create or replace function soft_delete_company()
    returns trigger
    language plpgsql
as
$$
begin
    update company set _is_deleted = true where id = old.id;
    update post set _is_deleted= true where company_id = old.id;
    update position set _is_deleted = true where company_id = old.id;
    delete from position_reduced where id in (select id from position where company_id = old.id);
    delete from position_history_reduced where position_id in (select id from position where company_id = old.id);
    update post_history set end_date=current_date where post_id in (select id from post where company_id = old.id);
    update position_history
    set end_date=current_date
    where position_id in (select id from position where company_id = old.id);
    return null;
end;
$$;

create trigger soft_delete_company_trigger
    before delete
    on company
    for each row
execute function soft_delete_company();

create or replace function soft_delete_post()
    returns trigger
    language plpgsql
as
$$
begin
    update post set _is_deleted = true where id = old.id;
    update post_history set end_date = CURRENT_DATE where post_id = old.id;
    return null;
end;
$$;

create trigger soft_delete_post_trigger
    before delete
    on post
    for each row
execute function soft_delete_post();

create or replace function soft_delete_position()
    returns trigger
    language plpgsql
as
$$
begin
    if (select parent_id from position where id = old.id) is null then
        raise 'Unable to delete chief position';
    end if;
    set session_replication_role = replica;
    update position set parent_id = old.parent_id where parent_id = old.id;
    update position_reduced set parent_id = old.parent_id where parent_id = old.id;
    set session_replication_role = origin;
    update position set _is_deleted = true where id = old.id;
    update position_history set end_date = current_date where position_id = old.id and end_date is null;
    delete from position_history_reduced where position_id = old.id;
    return null;
end;
$$;

create trigger soft_delete_position_trigger
    before delete
    on position
    for each row
execute function soft_delete_position();

create or replace function close_previous_position_insert() returns trigger
    language plpgsql as
$$
begin
    if (new.end_date is null) then
        insert into position_history_reduced values (new.position_id, new.employee_id);
    end if;
    return new;
end;
$$;


create trigger close_previous_position_insert
    after insert
    on position_history
    for each row
execute function close_previous_position_insert();

create or replace function close_previous_position_update() returns trigger
    language plpgsql as
$$
begin
    if (old.end_date is null and new.end_date is not null) then
        delete
        from position_history_reduced
        where employee_id = old.employee_id
          and position_id = old.position_id;
    else
        update position_history_reduced
        set employee_id = new.employee_id,
            position_id = new.position_id
        where employee_id = old.employee_id
          and position_id = old.position_id;
    end if;
    return new;
end;
$$;

create trigger close_previous_position_update
    after update
    on position_history
    for each row
execute function close_previous_position_update();

create or replace function delete_position_history() returns trigger
    language plpgsql as
$$
begin
    if (old.end_date is null) then
        delete from position_history_reduced where employee_id = old.employee_id and position_id = old.position_id;
    end if;
    return old;
end;
$$;

-- drop trigger delete_position_history on position_history;

create trigger delete_position_history
    after delete
    on position_history
    for each row
execute function delete_position_history();

create or replace function close_previous_post_insert() returns trigger
    language plpgsql as
$$
begin
    update post_history
    set end_date=CURRENT_DATE
    where employee_id = new.employee_id
      and end_date is null
      and (select company_id from post where post_id = post.id) =
          (select company_id from post where post.id = new.post_id);
    return new;
end;
$$;


create trigger close_previous_post_insert
    before insert
    on post_history
    for each row
execute function close_previous_post_insert();

create or replace function update_employee_email() returns trigger
    language plpgsql as
$$
begin
    if (new.email != old.email) then
        update employee_reduced set email = new.email where id = old.id;
        update users set email = new.email where email = old.email;
    end if;
    return new;
end;
$$;

-- drop trigger update_employee_email on cp_test.public.employee_base;

create trigger update_employee_email
    after update
    on employee_base
    for each row
execute function update_employee_email();

create or replace function update_company_email() returns trigger
    language plpgsql as
$$
begin
    if (new.email != old.email) then

        update users set email = new.email where email = old.email;
    end if;
    return new;
end;
$$;

create trigger update_company_email
    before update
    on company
    for each row
execute function update_company_email();

create or replace function insert_employee() returns trigger
    language plpgsql as
$$
begin
    insert into employee_reduced values (new.id, new.email);
    return new;
end;
$$;

-- drop trigger insert_employee on employee_base;

create trigger insert_employee
    after insert
    on employee_base
    for each row
execute function insert_employee();

create or replace function insert_position() returns trigger
    language plpgsql as
$$
begin
    insert into position_reduced values (new.id, new.parent_id);
    return new;
end;
$$;

-- drop trigger insert_position on position;

create trigger insert_position
    after insert
    on position
    for each row
execute function insert_position();

