create table if not exists employee_base
(
    id         uuid primary key default gen_random_uuid(),
    full_name  text                not null,
    phone      varchar(16) unique  not null check ( phone ~ '^\+[0-9]{1,3}[0-9]{4,14}$' ),
    email      varchar(255) unique not null check ( email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' ),
    birth_date date                not null check ( birth_date < CURRENT_DATE ),
    photo      text unique      default null,
    duties     jsonb            default null
);

alter table employee_base
    enable row level security;

create table if not exists employee_reduced
    (
        id         uuid references employee_base (id) on delete cascade,
        email      varchar(255) unique not null check ( email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' )
);

create temporary view employee as
select *, extract(year from age(employee_base.birth_date)) as age
from employee_base;

create table if not exists education
(
    id              uuid primary key default gen_random_uuid(),
    employee_id     uuid references employee_base (id) on delete cascade,
    institution     text not null,
    education_level text not null check ( education_level in (
                                                              'Высшее (бакалавриат)',
                                                              'Высшее (магистратура)',
                                                              'Высшее (специалитет)',
                                                              'Среднее профессиональное (ПКР)',
                                                              'Среднее профессиональное (ПССЗ)',
                                                              'Программы переподготовки',
                                                              'Курсы повышения квалификации'
        ) ),
    study_field     text not null,
    start_date      date not null check ( start_date < CURRENT_DATE ),
    end_date        date
);

alter table education
    enable row level security;

create table if not exists company
(
    id                uuid primary key             default gen_random_uuid(),
    title             text unique         not null,
    registration_date date                not null check ( registration_date <= CURRENT_DATE ),
    phone             varchar(16) unique  not null check ( phone ~ '^\+[0-9]{1,3}[0-9]{4,14}$' ),
    email             varchar(255) unique not null check ( email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' ),
    inn               varchar(10) unique  not null check ( inn ~ '^[0-9]{10}$' ),
    kpp               varchar(9) unique   not null check ( kpp ~ '^[0-9]{9}$' ),
    ogrn              varchar(13) unique  not null check ( ogrn ~ '^[0-9]{13}$' ),
    address           text                not null,
    _is_deleted       bool                not null default false
);

create table if not exists post
(
    id          uuid primary key        default gen_random_uuid(),
    title       text           not null,
    salary      numeric(10, 2) not null check ( salary > 0 ),
    company_id  uuid references company (id) on delete cascade,
    _is_deleted bool           not null default false
);

create table if not exists position
(
    id          uuid primary key default gen_random_uuid(),
    parent_id   uuid references position (id),
    title       text not null,
    company_id  uuid references company (id) on delete cascade,
    _is_deleted bool not null    default false
);

create table if not exists position_reduced
(
  id uuid primary key,
  parent_id uuid references position_reduced(id)
);

create table if not exists post_history
(
    post_id     uuid references post (id),
    employee_id uuid references employee_base (id) on delete cascade,
    start_date  date not null check ( start_date < CURRENT_DATE ),
    end_date    date check ( end_date <= CURRENT_DATE )
);

alter table post_history
    enable row level security;

create table if not exists position_history
(
    position_id uuid references position (id),
    employee_id uuid references employee_base (id) on delete cascade,
    start_date  date not null check ( start_date < CURRENT_DATE ),
    end_date    date check ( end_date <= current_date )
);

alter table position_history
    enable row level security;

create table if not exists position_history_reduced(
    position_id uuid references position (id),
    employee_id uuid references employee_base(id) on delete cascade
);

create table if not exists score_story
(
    id               uuid primary key     default gen_random_uuid(),
    employee_id      uuid references employee_base (id) on delete cascade,
    author_id        uuid        references employee_base (id) on delete set null,
    position_id      uuid references position (id),
    created_at       timestamptz not null default now(),
    efficiency_score int check ( efficiency_score > 0 and efficiency_score < 6 ),
    engagement_score int check ( engagement_score > 0 and engagement_score < 6 ),
    competency_score int check ( competency_score > 0 and competency_score < 6 )
);

alter table score_story
    enable row level security;

drop table if exists users;

create table if not exists users(
    email varchar(255) primary key check( email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' ),
    password text not null unique,
    salt text not null,
    role text not null
);

select * from users;

select * from employee_base;