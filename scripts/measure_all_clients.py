import json
import os
import random
import subprocess
import sys

import psutil
import requests
import time
import statistics

if os.path.exists("../measures/clients.json"):
    with open("../measures/clients.json", 'r') as f:
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

with open("../measures/cache_clients.json", "w") as f:
    for i in range(5, 51, 5):
        print(f"Clients count {i}")
        processes = list()
        current_dir = os.path.dirname(os.path.abspath(__file__))
        for j in range(i):
            print(f"Client started {j}")
            process = subprocess.Popen(
                [sys.executable, os.path.join(current_dir, 'measure_client.py'), f"{i}_{j}"],
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                cwd = current_dir,
                text=True
            )
            processes.append(process)

        time.sleep(90)

        for process in processes:
            process.kill()
            process.wait()

        time.sleep(1)

        max_first_time = -1
        min_first_time = -1
        for j in range(i - 1, -1, -1):
            with open(f"../measures/client_{i}_{j}.txt", "r") as f1:
                strs = f1.readlines()
                cur_first_time = float(strs[0].split()[0])
                cur_last_time = float(strs[-1].split()[0])
                max_first_time = max(max_first_time, cur_first_time)
                if min_first_time == -1:
                    min_first_time = cur_last_time
                else:
                    min_first_time = min(min_first_time, cur_last_time)

        cnt = 0
        summ = 0
        for j in range(i):
            with open(f"../measures/client_{i}_{j}.txt", "r") as f1:
                strs = f1.readlines()
                for s in strs:
                    start_time, time_delta = map(float, s.split())
                    if max_first_time <= start_time <= min_first_time:
                        summ += time_delta
                        cnt += 1
        if cnt != 0:
            if str(i) in data:
                data[str(i)].append( summ / cnt )
            else:
                data[str(i)] = [ summ / cnt ]

    f.write(json.dumps(data))