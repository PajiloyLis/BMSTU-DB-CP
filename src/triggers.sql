create or replace function check_uniqueness_post_title()
    returns trigger
    language plpgsql
as
$$
begin
    if exists (select title from post where title = new.title and company_id = new.company_id) then
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
    if exists (select title from position where title = new.title and company_id = new.company_id) then
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
                 and (end_date is null or end_date > new.start_date)) then
        raise exception 'Position almost occupied by employee %', employee_id;
    end if;
    return new;
end;
$$;

create trigger check_non_crossing_positions_trigger
    before insert or update
    on position_history
    for each row
execute function check_non_crossing_positions_history();


create or replace function reassignment_positions()
    returns trigger
    language plpgsql
as
$$
begin
    update position set parent_id = old.parent_id where parent_id = old.id;
    update position set _is_deleted = true where id = old.id;
    return null;
end;
$$;

create trigger reassignment_positions_trigger
    before delete
    on position
    for each row
execute function reassignment_positions();


create or replace function protect_column_parent_id()
    returns trigger
    language plpgsql
as
$$
begin
    if old.parent_id is distinct from new.parent_id and
       (select count(*) from position where parent_id = old.position_id) > 0 then
        raise exception 'Parent id cannot be changed directly for employees with subordinates, use the special functions: change_parent_id_with_subordinates(uuid, uuid), change_parent_id_without_subordinates(uuuid, uuid)';
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
    if new.created_at - (select max(score_story.created_at)
                         from score_story
                         where employee_id = new.employee_id
                           and position_id = new.position_id) < interval '1 month' then
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
    return null;
end;
$$;

create trigger soft_delete_post_trigger
    before delete
    on post
    for each row
execute function soft_delete_post();
