import psycopg2
import uuid
from datetime import datetime, timedelta
import random

# Настройки подключения к PostgreSQL
DB_CONFIG = {
    'host': 'localhost',
    'database': 'cp_test',
    'user': 'postgres',
    'password': 'postgres',
    'port': '5432'
}

# Случайные данные для генерации
INSTITUTIONS = [
    "Московский государственный университет",
    "Санкт-Петербургский государственный университет",
    "Новосибирский государственный университет",
    "Московский физико-технический институт",
    "Высшая школа экономики",
    "Московский инженерно-физический институт",
    "Санкт-Петербургский политехнический университет",
    "Уральский федеральный университет",
    "Казанский федеральный университет",
    "Московский авиационный институт"
]

STUDY_FIELDS = [
    "Информационные технологии",
    "Математика",
    "Физика",
    "Химия",
    "Биология",
    "Экономика",
    "Менеджмент",
    "Юриспруденция",
    "Медицина",
    "Инженерия",
    "Психология",
    "Лингвистика"
]

EDUCATION_LEVELS = [
    'Высшее (бакалавриат)',
    'Высшее (магистратура)',
    'Высшее (специалитет)',
    'Среднее профессиональное (ПКР)',
    'Среднее профессиональное (ПССЗ)',
    'Программы переподготовки',
    'Курсы повышения квалификации'
]

# Здесь добавьте ваши employee_id
EMPLOYEE_IDS = [
"bad8a5a0-ec08-412e-8f19-0d9e993d5651",
"bf8732c8-208b-4c1e-af7f-a996eb0f1061",
"5488e8d0-bd5b-46f7-b0bc-d94076add996",
"33dea239-2610-49d9-8f33-748f62f04e74",
"c7cabdc2-d5c7-4b0b-b607-fe7e3774bfab",
"a2c7e633-3b34-4143-801a-65fe6f700364",
"ed01868b-f88b-4afe-a9bb-5284e00230cc",
"21e41d95-b4e3-4599-bd8f-944dde04f6cb",
"07a33f8c-3fd4-43be-a10d-9ab824301689",
"9742cf5e-0880-4f57-a0de-1d3bb315f01b",
"d1e498e0-2d34-49f3-896e-5309e31e6ecb",
"156df69b-fc84-4149-a733-7b70d1b4a111",
"67fa667a-2fc8-41b9-b710-ace86f5e56fc",
"d87c8b99-54f5-45cb-a639-ba959293023e",
"8aab8056-cf47-43d4-9cd7-16557089b4ea",
"6b30c1d6-5dc9-42e5-817c-41ec6a22659d",
"a8941a92-c908-4426-8e69-a896196467db",
"f9f1add9-0321-4917-8f92-271a2204df5d",
"3f6fe29a-9338-449b-a6b3-8be82ef24f8e",
"707eaedc-b284-4c7e-9ef8-ac8b0691f974",
"28786a1b-d0c5-404e-be11-399cb8dcf4e9",
"21cc75fe-63ea-43c4-8232-c93ffccd30b0",
"53f908b8-5353-403c-a5c5-9c3f3fc92392",
"91e806c5-cac6-4789-a8fe-f94c6ca9c545",
"651f03c3-6581-4636-9602-a6c17e1eca12",
"246e7d6b-6774-476e-ad67-c2e5d0e52b6b",
"3734ba99-a18b-4eda-a827-4b4881de5195",
"47749086-988c-443f-9e4b-e49716cd01fd",
"2f0083df-d72c-4c50-a9ea-72c6e742537d",
"9d24ba0a-6811-493f-8f0b-438667cf319a",
"6ff7e9d8-3c9f-4d09-82ef-f4f963972f88",
"2c354bc1-5c3d-41b3-b406-4540b3fac852",
"8cd1cbaf-e589-4009-b74b-797d5be8ca33",
"2a1102ab-bb9f-40ce-9155-10e25e35f364",
"af8d1940-6755-44f1-becf-eafcdfba3457",
"30bab7ef-90c2-4b2f-84e1-8c36ca13d09a",
"eab25f5b-ed35-4925-86de-83a96698187f",
"e91ba438-a838-476a-9c05-070116511640",
"93316aa7-6909-4152-8ea4-09839ba4f25a",
"bb12a3dc-5d3b-4fb9-8a73-acddee0e9fae",
"cb16721f-b794-4cf9-91de-124cd4f9b8f1",
"4889eebd-778a-4947-ae1b-a50f4688731d",
"7b5fc9f9-2e4c-43a1-b6d2-f0b5edf43b96",
"0581f28e-c41a-4cb2-9d07-1efd86700e10",
"42d43b62-9d1e-4a75-98b6-cf48f6e76aeb",
"446a4190-0a9a-49db-b205-2099138c15ef",
"d82bbccc-7456-445e-9834-eeffad9d5a0e",
"68d5fa2e-c914-42b6-a4ae-12416b539b08",
"ad5887f2-0bec-4c4a-b8e0-318fb303d99e",
"4b5377fa-cb51-4954-8552-54acb626c108"
]  # Заполните своими UUID


def generate_education_data(num_records):
    """Генерирует данные для таблицы education"""
    data = []

    for _ in range(num_records):
        # Случайный employee_id (если массив не пустой)
        employee_id = random.choice(EMPLOYEE_IDS) if EMPLOYEE_IDS else None

        institution = random.choice(INSTITUTIONS)
        education_level = random.choice(EDUCATION_LEVELS)
        study_field = random.choice(STUDY_FIELDS)

        # Генерация дат
        start_year = random.randint(1990, 2020)
        start_month = random.randint(1, 12)
        start_day = random.randint(1, 28)
        start_date = datetime(start_year, start_month, start_day)

        # Для некоторых записей end_date может быть None (еще учатся)
        if random.random() < 0.8:  # 80% записей имеют end_date
            duration_years = random.randint(2, 6)
            end_date = start_date + timedelta(days=365 * duration_years)
            # Убедимся, что end_date не в будущем
            if end_date > datetime.now():
                end_date = datetime.now() - timedelta(days=random.randint(1, 365))
            end_date = end_date.date()
        else:
            end_date = None

        data.append({
            'employee_id': employee_id,
            'institution': institution,
            'education_level': education_level,
            'study_field': study_field,
            'start_date': start_date.date(),
            'end_date': end_date
        })

    return data


def insert_education_data(data):
    """Вставляет данные в таблицу education"""
    try:
        # Подключение к базе данных
        conn = psycopg2.connect(**DB_CONFIG)
        cursor = conn.cursor()

        # SQL запрос для вставки
        insert_query = """INSERT INTO education (employee_id, institution, education_level, study_field, start_date, end_date)
        VALUES ( \
                       %s, \
                       %s, \
                       %s, \
                       %s, \
                       %s, \
                       %s \
                       ) \
                       """

        # Подготовка данных для вставки
        records = [
            (item['employee_id'], item['institution'], item['education_level'],
             item['study_field'], item['start_date'], item['end_date'])
            for item in data
        ]

        # Выполнение вставки
        cursor.executemany(insert_query, records)
        conn.commit()

        print(f"Успешно вставлено {len(data)} записей")

    except Exception as e:
        print(f"Ошибка при вставке данных: {e}")
        if conn:
            conn.rollback()
    finally:
        if cursor:
            cursor.close()
        if conn:
            conn.close()


def main():
    # Количество записей для генерации
    num_records = 5000

    print("Генерация данных...")
    education_data = generate_education_data(num_records)

    print("Вставка данных в базу...")
    insert_education_data(education_data)

    # Вывод примера сгенерированных данных
    # print("\nПример сгенерированных данных:")
    # for i, item in enumerate(education_data[:3]):
    #     print(f"{i + 1}. {item}")


if __name__ == "__main__":
    # Перед запуском заполните EMPLOYEE_IDS реальными UUID из вашей базы
    # EMPLOYEE_IDS = ['123e4567-e89b-12d3-a456-426614174000'), ...]

    main()