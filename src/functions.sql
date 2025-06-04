create or replace function get_subordinates_by_id(start_id uuid)
    returns table
            (
                id        uuid,
                parent_id uuid,
                title     text,
                level     integer
            )
AS
$$
begin
    return query
        with recursive hierarchy as (select p.id,
                                            p.parent_id,
                                            p.title,
                                            0 as level
                                     from position as p
                                     where p.id = start_id

                                     union all

                                     select p.id,
                                            p.parent_id,
                                            p.title,
                                            h.level + 1
                                     from position p
                                              join hierarchy as h on p.parent_id = h.id)
        SELECT *
        from hierarchy;
END;
$$ LANGUAGE plpgsql;



create or replace function get_subordinates_by_position(start_position text)
    returns table
            (
                id        uuid,
                parent_id uuid,
                title     text,
                level     integer
            )
AS
$$
begin
    return query
        with recursive hierarchy as (select p.id,
                                            p.parent_id,
                                            p.title,
                                            0 as level
                                     from position as p
                                     where p.title = start_position

                                     union all

                                     select p.id,
                                            p.parent_id,
                                            p.title,
                                            h.level + 1
                                     from position p
                                              join hierarchy as h on p.parent_id = h.id)
        SELECT *
        from hierarchy;
END;
$$ LANGUAGE plpgsql;

set row_security = off;

SHOW row_security;

drop function if exists get_current_subordinates_id_by_employee_id(head_employee_id uuid);

create or replace function get_current_subordinates_id_by_employee_id(head_employee_id uuid)
    returns table
            (
                employee_id uuid,
                position_id uuid,
                parent_id   uuid,
                title       text,
                level       int
            )
AS
$$
declare
    manager_position_id uuid;
begin
    -- Получаем текущую позицию сотрудника
    select position_history.position_id
    into manager_position_id
    from employee_base
             join position_history on employee_base.id = position_history.employee_id
    where employee_base.id = head_employee_id
      and position_history.end_date is null; -- Текущая позиция

    if manager_position_id is null then
        raise exception 'Employee with id % not found or has no current position', head_employee_id;
    end if;

    -- Возвращаем информацию о подчиненных
    return query
        select ph.employee_id, h.id, h.parent_id, h.title, h.level
        from (select public.position_history.employee_id, position_history.position_id
              from position_history
              where end_date is null) as ph
                 inner join (select * from get_subordinates_by_id(manager_position_id)) as h on h.id = ph.position_id;
end;
$$ LANGUAGE plpgsql;

select *
from get_current_subordinates_id_by_employee_id('2a1102ab-bb9f-40ce-9155-10e25e35f364');

drop function if exists get_current_subordinates_id_by_employee_id_rls(head_employee_id uuid);

create or replace function get_subordinates_by_id_rls(start_id uuid)
    returns table
            (
                id uuid
            )
    security definer
AS
$$
begin
    return query
        with recursive hierarchy as (select p.id
                                     from position_reduced as p
                                     where p.id = start_id

                                     union all

                                     select p.id
                                     from position_reduced p
                                              join hierarchy as h on p.parent_id = h.id)
        SELECT *
        from hierarchy;
END;
$$ LANGUAGE plpgsql;

create or replace function get_current_subordinates_id_by_employee_id_rls(head_employee_email text)
    returns table
            (
                employee_id uuid
            )
    security definer
AS
$$
declare
    manager_position_id uuid;
begin
    -- Получаем текущую позицию сотрудника
    select position_history_reduced.position_id
    into manager_position_id
    from employee_reduced
             join position_history_reduced on employee_reduced.id = position_history_reduced.employee_id
    where employee_reduced.email = head_employee_email; -- Текущая позиция

    if manager_position_id is null then
        return query (select id from employee_reduced where email = head_employee_email);
    end if;

    -- Возвращаем информацию о подчиненных
    return query (select ph.employee_id
                  from (select position_history_reduced.employee_id, position_history_reduced.position_id
                        from position_history_reduced) as ph
                           join (select * from get_subordinates_by_id_rls(manager_position_id)) as h
                                on h.id = ph.position_id);
end;
$$ LANGUAGE plpgsql;

-- select * from get_current_subordinates_id_by_employee_id('bad8a5a0-ec08-412e-8f19-0d9e993d5651');

select *
from get_current_subordinates_id_by_employee_id_rls('komarov@example.com');

create or replace function change_parent_id_with_subordinates(position_to_update_id uuid, new_parent_id uuid)
    returns void
    language plpgsql
    security definer
as
$$
begin

    if ((select count(*) from position where id = new_parent_id) = 0) then
        raise exception 'New parent id is not an employee';
    else
        update position set parent_id = new_parent_id where id = position_to_update_id;
        update position_reduced set parent_id = new_parent_id where id = position_to_update_id;
    end if;
end;
$$;

create or replace function change_parent_id_without_subordinates(position_to_update_id uuid, new_parent_id uuid)
    returns void
    language plpgsql
    security definer
as
$$
begin
    if ((select count(*) from position where id = new_parent_id) = 0) then
        raise exception 'New parent id is not an employee';
    else
        update position
        set parent_id = (select parent_id from position where id = position_to_update_id)
        where parent_id = position_to_update_id;
        update position set parent_id = new_parent_id where id = position_to_update_id;

        update position_reduced
        set parent_id = (select parent_id from position where id = position_to_update_id)
        where parent_id = position_to_update_id;
        update position_reduced set parent_id = new_parent_id where id = position_to_update_id;
    end if;
end;
$$;

select change_parent_id_with_subordinates('4eae2daf-0004-4000-8000-000000000004',
                                          '4eae2daf-0002-4000-8000-000000000002');
select *
from position
where company_id = '4eae2daf-8a11-4018-8c8a-5c5d813f596c';
