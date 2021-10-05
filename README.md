# NMQ_Model_practice
### 作業要求：
- 透過RabbitMQ 傳遞Message
- Worker 收Meddage 並處理(印出Message 即可)
- Worker 需要有能量同時執行多筆Message
- 定義"兩種"Message 類型，特定類型只能給指定的Unit 執行
### 作業要求示意圖
![unit_worker](https://user-images.githubusercontent.com/70478084/134273705-4035af6a-9358-46d9-a426-8eb3e200bf29.png)

## Structure


## Deployment
### Docker login
- ```docker loging docker-lab.build.91app.io```

### 前置準備 
於資料夾```BackgroundWorker```和```Dashboard```下創建```.env```檔案
- ```BackgroundWorker```下```.env```檔案內容範例
  - `RabbitMQ_Queue`: Taks queue in RabbitMQ
  - `RabbitMQ_Done_Queue`: Done Task queue in RabbitMQ
  - `TASK_THREAD_NUMBER`: 執行Task 的Thread 總數
  - `TASK_THREAD_PROCESS_TIME`: 執行Task 所花費的時間(ms)
```
RabbitMQ_UserName=root
RabbitMQ_Password=admin1234
RabbitMQ_VirtualHost=/
RabbitMQ_HostName=rabbitmqHost
RabbitMQ_Port=5672
RabbitMQ_TASK_Queue=task_queue
RabbitMQ_DONE_TASK_Queue=done_queue
TASK1_THREAD_NUMBER=3
TASK2_THREAD_NUMBER=5
TASK1_THREAD_PROCESS_TIME=600
TASK2_THREAD_RPOCESS_TIME=1000
TASK1_LIMIT=80
TASK2_LIMIT=50
```

- ```Dashboard```下```.env```檔案內容範例
```
PORT=3001
RabbitMQ_UserName=root
RabbitMQ_Password=admin1234
RabbitMQ_VirtualHost=/
RabbitMQ_HostName=rabbitmqHost
RabbitMQ_Port=5672
RabbitMQ_TASK_Queue=task_queue
RabbitMQ_DONE_TASK_Queue=done_queue
```

### 執行程式指令
```
docker-compose up
```

### 預覽結果
- 於本機端中，可以在任一瀏覽器的網址列輸入`localhost:3001`，查看NMQ Dashboard，並可於其中調整Task 1 和 Task 2 的執行數量。
- 於本機端中，可以在任一瀏覽器的網址列輸入`localhost:15672`，查看 RabbitMQ Dashboard。

### 刪除資源指令
```
docker-compose down
```
