import json
import statistics

import matplotlib.pyplot as plt
import seaborn

with open("../measures/cache_select.json", 'r') as f:
    data = json.load(f)

with open("../measures/select.json", 'r') as f:
    data_1 = json.load(f)

xs = [int(key) for key in data.keys()]
xs.sort()

ys = [data[str(x)]['avg_per_100'][1] for x in xs]

plt.plot(xs, ys, 'k--o')

print("cache select")
print(*xs)
print(*ys)

xs_1 = [int(key) for key in data_1.keys()]
xs_1.sort()

ys_1 = [statistics.mean(data_1[str(x)]['avg_per_100']) for x in xs]

plt.plot(xs_1, ys_1, 'k-*')

print("select")
print(*xs_1)
print(*ys_1)

plt.grid(True)

plt.legend(['Запросы без кэширования', "Запросы с кэшированием"])

plt.xlabel("Количество записей в таблице")

plt.ylabel("Время на 100 запросов, мс")

plt.title("Зависимость времени выполнения запроса от количества записей в таблице", wrap=True)

# plt.savefig("../measures/select.svg", format='svg')
# plt.show()



with open("../measures/cache_clients.json", 'r') as f:
    data_2 = json.load(f)

with open("../measures/clients.json", 'r') as f:
    data_3 = json.load(f)

xs_2 = [int(key) for key in data_2.keys()]
xs_2.sort()

ys_2 = [statistics.mean(data_2[str(x)]) for x in xs_2]

plt.plot(xs_2, ys_2, 'k--o')

print("cache clients")
print(*xs_2)
print(*ys_2)

xs_3 = [int(key) for key in data_3.keys()]
xs_3.sort()

ys_3 = [statistics.mean(data_3[str(x)]) for x in xs_3]

plt.plot(xs_3, ys_3, 'k-*')

print("clients")
print(*xs_3)
print(*ys_3)

plt.grid(True)

plt.legend(['Запросы без кэширования', "Запросы с кэшированием"])

plt.xlabel("Количество одновременно выполняемых запросов")

plt.ylabel("Время на 10 запросов, мс")

plt.title("Зависимость времени выполнения запроса от количества одновременно выполняемых запросов", wrap=True)
#
# plt.savefig("../measures/clients.svg", format = "svg")
# plt.show()

sum = 0
for i in range(len(ys_2)):
    sum+= ys_2[i]/ys_3[i]
print(sum/len(ys_2))
