import json
import os
import random
import subprocess
import sys

import psutil
import requests
import time
import statistics

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
]


def find_actual_port(pid, timeout=30):
    """Находит реальный порт, который слушает процесс"""
    start_time = time.time()

    while time.time() - start_time < timeout:
        try:
            process = psutil.Process(pid)
            connections = process.connections(kind='inet')

            for conn in connections:
                if conn.status == psutil.CONN_LISTEN and conn.laddr:
                    port = conn.laddr.port
                    print(f"Найден реальный порт: {port}")
                    return port

            print("Порт еще не обнаружен, ждем...")
            time.sleep(1)

        except (psutil.NoSuchProcess, psutil.AccessDenied):
            print("Процесс не найден")
            return None
        except Exception as e:
            print(f"Ошибка поиска порта: {e}")
            time.sleep(1)

    return None

def benchmark_requests(url, num_requests=1000):
    """
    Замеряет время выполнения указанного количества запросов к URL
    """
    times = []
    successful_requests = 0

    with requests.Session() as session:
        start_time = time.time()
        for i in range(num_requests):
            # print(i)
            try:
                response = session.get(url, headers={
                    "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJiZWxvdkBleGFtcGxlLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6ImVtcGxveWVlIiwiZXhwIjoxNzYwNjQ0OTQyLCJpc3MiOiJpc3N1ZXIiLCJhdWQiOiJhdWRpZW5jZSJ9.pH0fcik-6avRM6eR79-J-NIMWSKvr_5SiS_CadIthIs"})

                if response.status_code == 200:
                    successful_requests += 1
                else:
                    print(f"Запрос {i + 1}: Ошибка {response.status_code}")

            except Exception as e:
                print(f"Запрос {i + 1}: Ошибка - {e}")
                continue

        end_time = time.time()
    # Вывод результатов
    print("\n" + "=" * 50)
    print("РЕЗУЛЬТАТЫ ТЕСТИРОВАНИЯ:")
    print(f"Всего запросов: {num_requests}")
    print(f"Успешных запросов: {successful_requests}")
    print(f"Среднее время на 100 запросов: {(end_time - start_time) * 1000 / PER:.2f} мс")
    print(f"Общее время: {(end_time - start_time) * 1000:.2f} секунд")

    return {"avg_per_100": (end_time - start_time) * 1000 / PER, "total": (end_time - start_time)*1000}


if __name__ == "__main__":

    global PER
    num_requests = 1000
    PER = num_requests // 100

    if os.path.exists("../measures/select.json"):
        with open("../measures/select.json", 'r') as f:
            str_json_array = f.readlines()
        if len(str_json_array) != 0:
            str_json = ""
            for s in str_json_array:
                str_json = str_json + s
            data = json.loads(str_json)
        else:
            data = {}
    else:
        data = {}
    for i in range(1, 21):
    #     env = os.environ.copy()
    #     env['DOTNET_ROOT'] = '/usr/bin/dotnet'
    #     current_dir = os.path.dirname(os.path.abspath(__file__))
    #     binary_dir = os.path.join(current_dir, '..', 'Debug')
    #     binary_path = os.path.join(binary_dir, 'Project.HttpServer.dll')
    #
    #     Запуск с указанием рабочей директории
        # process = subprocess.Popen(
        #     ['dotnet', binary_path],
        #     cwd=binary_dir,
        #     env=env,
            # stdout=subprocess.PIPE,
            # stderr=subprocess.PIPE
        # )
        #
        # port = find_actual_port(process.pid)
        # if port is None:
        #     print("FUCK")
        #     exit()
        val = i*500
        print(val)
        target_url = f"http://localhost:5000/employeeEducations/9742cf5e-0880-4f57-a0de-1d3bb315f01b"
        os.system("python3 ./generate_education.py")
        results = benchmark_requests(target_url)
        for key in results.keys():
            if str(val) in data:
                if key in data[str(val)]:
                    data[str(val)][key].append(results[key])
                else:
                    data[str(val)][key]= [results[key]]
            else:
                data[str(val)] = {key: [results[key]]}
    # process.terminate()
    # process.wait()
    with open("../measures/select.json", 'w') as f:
        f.write(json.dumps(data))