drop function if exists  current_user_id();
CREATE FUNCTION current_user_id() RETURNS uuid AS $$
BEGIN
    RETURN (SELECT id FROM employee_base WHERE phone = current_user or email == current_user);
END;
$$ LANGUAGE plpgsql;

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


create role guest_user with
    login
    bypassrls ;
grant select on company, post, position to guest_user;

create user guest;
grant guest_user to guest;

create role employee_user with
    login;
grant select on all tables in schema public to employee_user;
grant insert, update on score_story to employee_user;
create policy employee_reading_policy on employee_base
    for select
    to employee_user
    using (id in (select employee_id from get_current_subordinates_id_by_employee_id(current_user_id())));

create policy employee_reading_policy on position_history
    for select
    to employee_user
    using (position_history.employee_id in (select * from get_current_subordinates_id_by_employee_id(current_user_id())));

create policy employee_reading_policy on post_history
    for select
    to employee_user
    using (post_history.employee_id in (select * from get_current_subordinates_id_by_employee_id(current_user_id())));

create policy employee_reading_policy on education
    for select
    to employee_user
    using (education.employee_id in (select * from get_current_subordinates_id_by_employee_id(current_user_id())));

create policy employee_reading_policy on employee_base
    for select
    to employee_user
    using (id in (select * from get_current_subordinates_id_by_employee_id(current_user_id())));


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

select current_user;