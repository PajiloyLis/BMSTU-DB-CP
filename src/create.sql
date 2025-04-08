create table if not exists employee_base
(
    id         uuid primary key default gen_random_uuid(),
    full_name  text         not null,
    phone      varchar(16) check ( phone ~ '^\+[0-9]{1,3}[0-9]{4,14}$' ),
    email      varchar(255) not null check ( email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' ),
    birth_date date         not null check ( birth_date < CURRENT_DATE ),
    photo      text             default null,
    duties     jsonb            default null
);



create temporary view employee as
select *, extract(year from age(employee_base.birth_date)) as age
from employee_base;

drop view employee;
select *
from employee;

insert into employee_base(full_name, phone, email, birth_date)
values ('Vasya', '+79991234567', 'vasyan@gmail.com', '12.01.2004'),
       ('Kolya', '+78939876543', 'kolyan@yandex.ru', '23.06.1999');

-- create table if not exists test(
--                                    created_at timestamptz default now()
-- );
--
-- create temporary view test_view as select *, now() - test.created_at as difference from test;
--
-- insert into test(created_at) values (now()), (now()), (now());
--
-- select * from test;
--
-- select * from test_view;
--
-- drop table test;
-- drop view test_view;