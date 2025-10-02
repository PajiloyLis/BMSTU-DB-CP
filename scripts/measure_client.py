import json
import os
import random
import subprocess
import sys

import psutil
import requests
import time
import statistics

client_num = sys.argv[1]
url = f"http://localhost:5000/employeeEducations/9742cf5e-0880-4f57-a0de-1d3bb315f01b"
print(f"Client {client_num} started")
timeout = 60
start = time.time()
with open(f"../measures/client_{client_num}.txt", "w") as f:
    with requests.Session() as session:
        while time.time() - start < timeout:
            try:
                start_time = time.time()
                for i in range(10):
                    response = session.get(url, headers={
                        "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJiZWxvdkBleGFtcGxlLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6ImVtcGxveWVlIiwiZXhwIjoxNzYwNjQ0OTQyLCJpc3MiOiJpc3N1ZXIiLCJhdWQiOiJhdWRpZW5jZSJ9.pH0fcik-6avRM6eR79-J-NIMWSKvr_5SiS_CadIthIs"})
                    if response.status_code != 200:
                        print(f"Ошибка {response.status_code}")
                end_time = time.time()

                f.write(f"{start_time} {(end_time-start_time)*1000}\n")

            except Exception as e:
                print(f"Ошибка - {e}")
                continue
