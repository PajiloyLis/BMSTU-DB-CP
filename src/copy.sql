-- copy employee_base (full_name, phone, email, birth_date, photo, duties) from '/tmp/employee.csv' delimiter ',' csv header;
copy employee_base (id, full_name, phone, email, birth_date, photo, duties) from '/tmp/employee_with_id.csv' delimiter ',' csv header;

select *
from employee_base;

-- COPY employee_base TO '/tmp/employee_with_id.csv' WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

select *
from employee;

copy education (id, employee_id, institution, education_level, study_field, start_date,
                end_date) from '/tmp/education_with_id.csv' delimiter ',' csv header NULL 'NULL';
-- copy education (employee_id, institution, education_level, study_field, start_date, end_date) from '/tmp/education.csv' delimiter ',' csv header NULL 'NULL';

select *
from education;

-- COPY education TO '/tmp/education_with_id.csv'
--     WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

select *
from employee_base
         join education on employee_base.id = education.employee_id
where not (education.education_level::text like '%Высшее%')
  and education.education_level::text not in
      ('Среднее профессиональное (ПССЗ)', 'Программы переподготовки', 'Курсы повышения квалификации');

select *
from employee_base
where employee_base.id not in (select employee_id from education);

-- copy company (title, registration_date, phone, email, inn, kpp, ogrn, address) from '/tmp/company.csv' delimiter ',' csv header;
copy company (id, title, registration_date, phone, email, inn, kpp, ogrn, address) from '/tmp/company_with_id.csv' delimiter ',' csv header;
select *
from company;

-- COPY company TO '/tmp/company_with_id.csv'
--     WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

-- copy post (title, salary, company_id) from '/tmp/post.csv' delimiter ',' csv header;
copy post (id, title, salary, company_id) from '/tmp/post_with_id.csv' delimiter ',' csv header;
select *
from post;

-- COPY post TO '/tmp/post_with_id.csv'
--     WITH CSV HEADER DELIMITER ',' ENCODING 'UTF-8';

copy position (id, title, parent_id, company_id) from '/tmp/position.csv' delimiter ',' csv header NULL 'null';

select *
from position;

copy post_history (post_id, employee_id, start_date, end_date) from '/tmp/post_history.csv' delimiter ',' csv header NULL 'null';

select *
from post_history;

select id
from employee_base
where id not in (select employee_id from post_history);

select min(start_date)
from post_history;

select distinct company.title
from company
         join post on company.id = post.company_id
where post.id in (select post_id from post_history);

select id
from post
where id not in (select post_id from post_history);

select post_id from post_history where post_id not in (select id from post);
