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

drop function if exists get_subordinates_by_employee_name(employee_name text);

create or replace function get_subordinates_by_employee_name(employee_name text)
    returns table
            (full_name text)
AS
$$
declare
    manager_position_id uuid;
begin
    -- Получаем текущую позицию сотрудника
    select position_id into manager_position_id
    from employee_base
    join position_history on employee_base.id = position_history.employee_id
    where employee_base.full_name = employee_name
    and position_history.end_date is null;  -- Текущая позиция

    if manager_position_id is null then
        raise exception 'Employee with name % not found or has no current position', employee_name;
    end if;

    -- Возвращаем информацию о подчиненных
    return query
    select employee_base.full_name from employee_base join position_history on employee_base.id = position_history.employee_id
    where position_history.position_id in (
        select res.id
        from get_subordinates_by_id(manager_position_id) as res
    ) and end_date is null;
end;
$$ LANGUAGE plpgsql;

create or replace function change_parent_id_with_subordinates(position_to_update_id uuid, new_parent_id uuid)
    returns void
    language plpgsql
as
$$
begin
    if ((select count(*) from position where id = new_parent_id) = 0) then
        raise exception 'New parent id is not an employee';
    else
        update position set parent_id = new_parent_id where id = position_to_update_id;
    end if;
end;
$$;

create or replace function change_parent_id_without_subordinates(position_to_update_id uuid, new_parent_id uuid)
    returns void
    language plpgsql
as
$$
begin
    if ((select count(*) from position where id = new_parent_id) > 0) then
        raise exception 'New parent id is not an employee';
    else
        update position
        set parent_id = (select parent_id from position where id = position_to_update_id)
        where parent_id = position_to_update_id;
        update position set parent_id = new_parent_id where id = position_to_update_id;
    end if;
end;
$$;
