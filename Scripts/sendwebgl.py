import os
import paramiko

instruct = "scp -r Build/WebGL/Build django:~/sgs/game/static"
os.system(instruct)


# 实例化SSHClient  
ssh_client = paramiko.SSHClient()
ssh_client.set_missing_host_key_policy(paramiko.AutoAddPolicy())  
# 连接SSH服务端，以用户名和密码进行认证 ，调用connect方法连接服务器
ssh_client.connect(hostname='123.56.19.80', port=20000, username='wyx')
# 打开一个Channel并执行命令  结果放到stdout中，如果有错误将放到stderr中
stdin, stdout, stderr = ssh_client.exec_command('''
source /home/wyx/sgs/env/bin/activate
echo yes | python3 /home/wyx/sgs/manage.py collectstatic
''') 
# stdout 为正确输出，stderr为错误输出，同时是有1个变量有值   
# 打印执行结果  
print(stdout.read().decode('utf-8'))
print(stderr.read().decode('utf-8'))
# 关闭SSHClient连接 
ssh_client.close()


# # 获取Transport实例
# tran = paramiko.Transport(("123.56.19.80", 20000))

# # 连接SSH服务端，使用password
# tran.connect(username="wyx")

# # 获取SFTP实例
# sftp = paramiko.SFTPClient.from_transport(tran)

# # 设置上传的本地/远程文件路径
# localpath = "Build/WebGL"
# remotepath = "~/sgs/game/static"

# # 执行上传动作
# sftp.put(localpath, remotepath)
# # 执行下载动作
# sftp.get(remotepath, localpath)

# tran.close()
