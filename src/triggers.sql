create or replace function check_uniqueness_post_title()
returns trigger
language plpgsql
as $$
begin
    if exists (select title from post where title = new.title and company_id = new.company_id) then
        raise exception 'Post title must be unique in a company';
    end if;
    return new;
end;
$$;

create trigger check_uniqueness_post_title_trigger
before insert or update on post
for each row
execute function check_uniqueness_post_title();

create or replace function check_uniqueness_position_title()
returns trigger
language plpgsql
as $$
begin
    if exists (select title from position where title = new.title and company_id = new.company_id) then
        raise exception 'Position title must be unique in a company';
    end if;
    return new;
end;
$$;

create trigger check_uniqueness_position_title_trigger
before insert or update on position
for each row
execute function check_uniqueness_position_title();

create or replace function check_non_crossing_positions_history()
returns trigger
language plpgsql
as $$
begin
    if exists (select start_date, end_date, position_id, employee_id from position_history where position_id = new.position_id and start_date <= new.start_date and (end_date is null or end_date > new.start_date)) then
        raise exception 'Position almost occupied by employee %', employee_id;
    end if;
    return new;
end;
$$;

create trigger check_non_crossing_positions_trigger
before insert or update on position_history
for each row
execute function check_non_crossing_positions_history();


create or replace function reassignment_positions()
returns trigger
language plpgsql
as $$
begin
    update position set parent_id = old.parent_id where parent_id = old.id;
    return old;
end;
$$;

create trigger reassignment_positions_trigger
before delete on position_history
for each row
execute function reassignment_positions();

