drop function if exists  current_user_id();

SET app.current_user_name = 'komarov@example.com';

-- RESET app.current_user_name;

CREATE or replace FUNCTION current_user_id() RETURNS uuid security definer AS $$
BEGIN
    RETURN (SELECT id FROM employee_base WHERE email = current_setting('app.current_user_name')::text);
END;
$$ LANGUAGE plpgsql;

-- SELECT set_config('app.current_user_id', current_user_id()::text, false);

-- select current_setting('app.current_user_id');

select * from current_user_id();

-- SELECT current_setting('app.current_user_name');

-- select * from employee_base;


create role admin_user with
    login
    password 'admin_pass'
    superuser
    createdb
    createrole
    replication
    bypassrls;
create user admin;

grant admin_user to admin;

drop role guest_user;
create role guest_user with
    login
    password 'guest_pass';
grant select on company, post, position, public.users to guest_user;
grant insert on public.users to guest_user;
grant execute on function get_subordinates_by_id to guest_user;

-- revoke all on all functions in schema public FROM guest_user;
-- revoke all on all tables in schema public from guest_user;

-- drop user guest;
create user guest with password 'guest_pass';
grant guest_user to guest;


revoke all on all tables in schema public from employee_user;

revoke all on all functions in schema public from employee_user;
drop role employee_user;

create role employee_user with
    login password 'employee_pass';
grant select on all tables in schema public to employee_user;
revoke select on employee_reduced, position_reduced, position_history_reduced from employee_user;
grant insert, update on score_story to employee_user;
grant execute on function get_subordinates_by_position to employee_user;
grant execute on function get_subordinates_by_id to employee_user;
grant execute on function get_current_subordinates_id_by_employee_id to employee_user;
grant execute on function get_current_subordinates_id_by_employee_id_rls to employee_user;
grant execute on function get_subordinates_by_id_rls to employee_user;

drop policy if exists employee_reading_policy on employee_base;
drop policy if exists employee_reading_policy on education;
drop policy if exists employee_reading_policy on position_history;
drop policy if exists employee_reading_policy on post_history;

create policy employee_reading_policy on position_history
    for select
    to employee_user
    using (employee_id in(select * from get_current_subordinates_id_by_employee_id_rls((SELECT current_setting('app.current_user_name')))));

create policy employee_reading_policy on post_history
    for select
    to employee_user
    using (employee_id in(select * from get_current_subordinates_id_by_employee_id_rls((SELECT current_setting('app.current_user_name')))));

create policy employee_reading_policy on education
    for select
    to employee_user
    using (employee_id in(select * from get_current_subordinates_id_by_employee_id_rls((SELECT current_setting('app.current_user_name')))));

create policy score_reading_policy on score_story
    for select
    to employee_user
    using (employee_id in(select * from get_current_subordinates_id_by_employee_id_rls((SELECT current_setting('app.current_user_name')))));

create policy employee_reading_policy on employee_base
    for select
    to employee_user
    using (id in(select * from get_current_subordinates_id_by_employee_id_rls((SELECT current_setting('app.current_user_name')))));

drop user employee;
create user employee with password 'employee_pass';
grant employee_user to employee;

SELECT
    grantee,
    table_catalog AS database,
    table_schema AS schema,
    table_name AS object,
    string_agg(privilege_type, ', ') AS privileges
FROM
    information_schema.table_privileges
GROUP BY
    grantee, table_catalog, table_schema, table_name
ORDER BY
    grantee, table_schema, table_name;

SELECT proname, proacl
FROM pg_proc
WHERE pronamespace = 'public'::regnamespace;

select current_user;

select * from employee_base;