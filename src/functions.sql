create or replace function get_subordinates_by_id(start_id uuid)
returns table (
    id uuid,
    parent_id uuid,
    title text,
    level integer
) AS $$
begin
    return query
    with recursive hierarchy as (
        select
            p.id,
            p.parent_id,
            p.title,
            0 as level
        from position as p
        where p.id = start_id
        
        union all

        select
            p.id,
            p.parent_id,
            p.title,
            h.level + 1
        from position p
        join hierarchy as h on p.parent_id = h.id
    )
    SELECT * from hierarchy;
END;
$$ LANGUAGE plpgsql;

create or replace function get_subordinates_by_position(start_position text)
returns table (
    id uuid,
    parent_id uuid,
    title text,
    level integer
) AS $$
begin
    return query
    with recursive hierarchy as (
        select
            p.id,
            p.parent_id,
            p.title,
            0 as level
        from position as p
        where p.title = start_position

        union all

        select
            p.id,
            p.parent_id,
            p.title,
            h.level + 1
        from position p
        join hierarchy as h on p.parent_id = h.id
    )
    SELECT * from hierarchy;
END;
$$ LANGUAGE plpgsql;

select * from position;

select * from get_subordinates_by_id('ac372e9b-2292-4354-bd4d-5c7470b54850');

select * from get_subordinates_by_position('Финансовый директор');