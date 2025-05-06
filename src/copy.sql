copy employee_base(id, full_name, phone, email, birth_date) from '/tmp/employee.csv' delimiter ',' csv header;

select * from employee_base;

select * from employee;

copy education(id, employee_id, institution, education_level, study_field, start_date, end_date) from '/tmp/education.csv' delimiter ',' csv header;

select * from education;

copy company(id, title, registration_date, phone, email, inn, kpp, ogrn, address) from '/tmp/company.csv' delimiter ',' csv header;

select * from company;

copy post(id, title, salary, company_id) from '/tmp/posts.csv' delimiter  ',' csv header;

select * from post;

copy position(id, parent_id, title) from '/tmp/positions.csv' delimiter ',' csv header ;

select * from position;