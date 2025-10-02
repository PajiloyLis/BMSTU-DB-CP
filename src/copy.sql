-- copy employee_base (full_name, phone, email, birth_date, photo, duties) from '/tmp/employee.csv' delimiter ',' csv header;
copy employee_base (id, full_name, phone, email, birth_date, photo, duties) from '/tmp/employee_with_id.csv' delimiter ',' csv header;

insert into employee_reduced(id, email) (select id, email from employee_base);

-- COPY employee_base TO '/tmp/employee_with_id.csv' WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

copy education (id, employee_id, institution, education_level, study_field, start_date,
                end_date) from '/tmp/education_with_id.csv' delimiter ',' csv header NULL 'NULL';
-- copy education (employee_id, institution, education_level, study_field, start_date, end_date) from '/tmp/education.csv' delimiter ',' csv header NULL 'NULL';

-- COPY education TO '/tmp/education_with_id.csv'
--     WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

-- copy company (title, registration_date, phone, email, inn, kpp, ogrn, address) from '/tmp/company.csv' delimiter ',' csv header;
copy company (id, title, registration_date, phone, email, inn, kpp, ogrn, address) from '/tmp/company_with_id.csv' delimiter ',' csv header;

-- COPY company TO '/tmp/company_with_id.csv'
--     WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

-- copy post (title, salary, company_id) from '/tmp/post.csv' delimiter ',' csv header;
copy post (id, title, salary, company_id) from '/tmp/post_with_id.csv' delimiter ',' csv header;

-- COPY post TO '/tmp/post_with_id.csv'
--     WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

copy position (id, title, parent_id, company_id) from '/tmp/position.csv' delimiter ',' csv header NULL 'null';

insert into public.position_reduced (select id, parent_id from position);

copy post_history (post_id, employee_id, start_date, end_date) from '/tmp/post_history.csv' delimiter ',' csv header NULL 'null';

copy position_history (position_id, employee_id, start_date, end_date) from '/tmp/position_history.csv' delimiter ',' csv header NULL 'null';

insert into position_history_reduced(position_id, employee_id)  (select position_id, employee_id from position_history where end_date is null);

copy score_story (employee_id, author_id, position_id, created_at, efficiency_score, engagement_score, competency_score) from '/tmp/scores.csv' delimiter ',' csv header NULL 'null';

COPY users TO '/tmp/users.csv'
    WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

COPY cp_test.public.employee_reduced TO '/tmp/employee_reduced.csv'
    WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

COPY cp_test.public.position_history_reduced TO '/tmp/position_history_reduced.csv'
    WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

COPY cp_test.public.position_reduced TO '/tmp/position_reduced.csv'
    WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';
