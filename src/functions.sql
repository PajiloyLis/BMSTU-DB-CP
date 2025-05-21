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

select *
from position;

select *
from get_subordinates_by_id('ac372e9b-2292-4354-bd4d-5c7470b54850');

select *
from get_subordinates_by_position('Финансовый директор');