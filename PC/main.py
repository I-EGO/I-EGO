# -*- coding: cp949 -*-
from tkinter import *
from tkinter import filedialog
from tkinter import messagebox
from wrapper import TkDND
from socket import *
import socket as so
import tkinter
import time
import threading

msgs = []
addrs = []
user_ip = ''
UDPqueue = []
TCPqueue = []
running = True
connected = True


# UI 관련 스레드
class UI(threading.Thread):
    root = ''
    new = ''
    index = 0
    def run(self):
        # UDP 브로드캐스팅
        def broadcast():
            global UDPqueue
            global msgs
            global addrs
            msgs.clear()
            addrs.clear()
            UDPqueue.append('broadcast')
        
        # 대답을 한 기기 중에서 선택
        def select(msg, ip):
            global user_ip
            global UDPqueue
            global connected
            print('select ' + msg)
            user_ip = ip
            TCPqueue.clear()
            UDPqueue.append(msg)
            lbl = Label(Frame3, text='IP: ' + user_ip, bg="#777777", borderwidth=2, relief="solid", width=23,
                         anchor="w")
            lbl.grid(row=1, column=1, padx=10, sticky=W, columnspan='3')
            lbl.config(font=('', 15,))
            connected = False
            self.new.destroy()
        
        # 선택한 기기와 TCP연결
        def connect():
            global msgs
            global addrs
            msgs.clear()
            addrs.clear()
            broadcast()
            time.sleep(1)
            self.new = Toplevel()
            self.new.title('ip list')
            length = len(msgs)
            for i in range(0, length):
                lbl8 = Label(self.new, text=msgs[i])
                lbl8.grid(row=i, column=0, padx=10, pady=10)
                lbl9 = Label(self.new, text=addrs[i])
                lbl9.grid(row=i, column=1, padx=10, pady=10)
                btn8 = Button(self.new, text='select', command=lambda x=i: select(msgs[x], addrs[x]))
                btn8.grid(row=i, column=2, padx=10, pady=10)
        
        # 선택한 데이터 전송
        def senddata():
            global TCPqueue
            global connected
            text = text1.get(0, END)
            if connected:
                messagebox.showinfo('warnning', 'Disconnected')
            elif text1.size() == 0:
                messagebox.showinfo('warnning', 'Select file')
            elif screentype.get() == 0:
                messagebox.showinfo('warnning', 'Select type of screen')
            elif dirtype.get() == 0:
                messagebox.showinfo('warnning', 'Select type of direction')
            else:
                for list in text:
                    file = open(list, 'rb')
                    line = list.split('/')
                    length = len(line)
                    data = file.read()
                    TCPqueue.append(('data ' + line[length-1] + ' ' + str(screentype.get()) + ' '
                                     + str(dirtype.get()) + ' ' + str(len(data))).encode())
                    TCPqueue.append(data)
                    TCPqueue.append('end'.encode())
                    delay = text2.get()
                    if delay == '':
                        delay = '0'
                    time.sleep(int(delay))
                    print('p')
        
        # PC에서 파일 선택
        def findfile():
            filename = filedialog.askopenfilename(initialdir="C:/", title="choose your file",
                                                  filetypes=(("jpeg files", "*.jpg"), ("mp4 files", "*.mp4")))
            text1.insert(str(self.index), filename)
            self.index += 1

        def deletefile():
            self.index -= 1
            for list in text1.curselection()[::-1]:
                print(list)
                text1.delete(list)
            self.index = text1.size()

        # 녹화시작 전송
        def startrecording():
            global TCPqueue
            global connected
            if connected:
                messagebox.showinfo('warnning', 'Disconnected')
            else:
                TCPqueue.append('start'.encode())

        # 녹화종료 전송
        def stoprecording():
            global TCPqueue
            global connected
            if connected:
                messagebox.showinfo('warnning', 'Disconnected')
            else:
                TCPqueue.append('stop'.encode())

        # 분석결과 전송
        def stopapp():
            global TCPqueue
            global connected
            if connected:
                messagebox.showinfo('warnning', 'Disconnected')
            else:
                TCPqueue.append('loading'.encode())
                time.sleep(5)
                TCPqueue.append('result'.encode())

        # 드래그 이벤트
        def drop(event):
            event.widget.insert(str(self.index), event.data[1:len(event.data) - 1])
            self.index += 1

        self.root = Tk()
        self.root["bg"] = "#777777"
        screentype = tkinter.IntVar()
        dirtype = tkinter.IntVar()
        self.root.title('I-EGO')
        dnd = TkDND(self.root)

        Frame1 = Frame(self.root, height=200, bg="#777777")
        title = Label(Frame1, text='I-EGO', bg="#777777")
        title.pack()
        title.config(font=('', 30,))
        Frame1.pack(side=TOP, padx=10, pady=10)

        Frame2 = Frame(self.root, height=200, bg="#777777")

        Framel = Frame(Frame2, height=200, bg="#777777")
        lbl1 = Label(Framel, text='select file', bg="#777777")
        lbl1.pack(pady=10)
        lbl1.config(font=('', 20,))
        scroll = Scrollbar(Framel)
        scroll.pack(side=RIGHT, fill=Y)
        text1 = Listbox(Framel, width=50, yscrollcommand=scroll.set, selectmode=MULTIPLE, selectbackground="#333333")
        text1.pack(side=LEFT, fill=Y)
        dnd.bindtarget(text1, drop, 'text/uri-list')
        Framel.pack(side=LEFT, fill=Y)

        Framer = Frame(Frame2, height=200, bg="#777777")
        blink = Label(Framer, width=2, height=3, bg="#777777")
        blink.grid(row=0, column=0)
        btnAdd = Button(Framer, text='+', command=findfile, width=2, height=1)
        btnAdd.grid(row=1, column=0)
        btnDelete = Button(Framer, text='-', command=deletefile, width=2, height=1)
        btnDelete.grid(row=2, column=0)
        Framer.pack(side=RIGHT, fill=Y, padx=5)

        Frame2.pack(side=LEFT, fill=Y, padx=10, pady=10)

        Frame3 = Frame(self.root, height=200, bg="#777777")
        lbl2 = Label(Frame3, text='kind of file', bg="#777777")
        lbl2.grid(row=0, column=0, padx=10, pady=10, columnspan='4')
        lbl2.config(font=('', 20,))
        lbl3 = Label(Frame3, text='IP: ' + user_ip, bg="#777777", borderwidth=2, relief="solid", width=23, anchor="w")
        lbl3.grid(row=1, column=1, padx=10, sticky=W, columnspan='3')
        lbl3.config(font=('', 15,))
        btn2 = Button(Frame3, text='connect', command=connect, width=15)
        btn2.grid(row=1, column=0, padx=10)
        btn3 = Button(Frame3, text='start recording', command=startrecording, width=15)
        btn3.grid(row=2, column=0, padx=10)
        btn4 = Button(Frame3, text='stop recording', command=stoprecording, width=15)
        btn4.grid(row=3, column=0, padx=10)
        btn5 = Button(Frame3, text='send', command=senddata, width=15)
        btn5.grid(row=4, column=0, padx=10, pady=10)
        btn6 = Button(Frame3, text='result', command=stopapp, width=15)
        btn6.grid(row=5, column=0, padx=10)

        cb1 = Radiobutton(Frame3, text='stereo', value=1, variable=screentype, bg="#777777")
        cb1.grid(row=2, column=1, padx=10, pady=10)
        cb2 = Radiobutton(Frame3, text='180 angle', value=2, variable=screentype, bg="#777777")
        cb2.grid(row=2, column=2, padx=10, pady=10)
        cb3 = Radiobutton(Frame3, text='360 angle', value=3, variable=screentype, bg="#777777")
        cb3.grid(row=2, column=3, padx=10, pady=10)
        cb4 = Radiobutton(Frame3, text='none', value=1, variable=dirtype, bg="#777777")
        cb4.grid(row=3, column=1, padx=10)
        cb5 = Radiobutton(Frame3, text='topbottom', value=2, variable=dirtype, bg="#777777")
        cb5.grid(row=3, column=2, padx=10)
        cb6 = Radiobutton(Frame3, text='leftright', value=3, variable=dirtype, bg="#777777")
        cb6.grid(row=3, column=3, padx=10)
        lbl4 = Label(Frame3, text='Delay', bg="#777777", borderwidth=2, relief="solid", width=7)
        lbl4.grid(row=4, column=1, padx=10)
        text2 = Entry(Frame3, borderwidth=2, relief="solid")
        text2.grid(row=4, column=2, padx=10, columnspan='2')
        Frame3.pack(side=RIGHT, fill=Y, padx=10, pady=10)
        self.root.mainloop()


# TCP 스레드
# TCPqueue에 들어있는 데이터 전달
class TCP(threading.Thread):
    global running

    def run(self):
        sock = socket(AF_INET, SOCK_STREAM)
        while running:
            try:
                global user_ip
                global TCPqueue
                global connected
                while running:
                    while running:
                        if user_ip != '':
                            break
                        else:
                            time.sleep(1)
                    if running:
                        ip = user_ip
                        connected = False
                    while running:
                        if ip != user_ip:
                            sock.close()
                            connected = True
                            break
                        if len(TCPqueue) != 0:
                            if TCPqueue[0] != '':
                                sock = socket(AF_INET, SOCK_STREAM)
                                sock.connect((ip, 7001))
                                sock.sendto(TCPqueue[0], (ip, 7001))
                                print(TCPqueue[0])
                                TCPqueue.pop(0)
                                sock.close()
                        else:
                            time.sleep(1)
            finally:
                sock.close()

    
# UDPread 스레드
# 통신이 올 때 까지 대기
# 통신이 오면 메세지와 주소 저장
class UDPread(threading.Thread):
    global running
    sock = socket(AF_INET, SOCK_DGRAM)

    def run(self):
        try:
            self.sock = socket(AF_INET, SOCK_DGRAM)
            self.sock.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
            self.sock.bind(('', 8002))
            global UDPqueue
            global msgs
            global addrs
            while running:
                s, addr = self.sock.recvfrom(1024)
                print(s)
                print(addr)
                msgs.append(s.decode())
                addrs.append(addr[0])
        except OSError as e:
            print(e)
        finally:
            self.sock.close()

    def stop(self):
        self.sock.shutdown(2)
        self.sock.close()


# UDPwrite 스레드
# IP 서브넷 획득
# UDPqueue에 들어있는 데이터 전달
class UDPwrite(threading.Thread):
    global running
    ip = so.gethostbyname(so.getfqdn())
    print(ip)
    list = ip.split('.')
    ip = list[0]+'.'+list[1]+'.'+list[2]+'.255'
    print(ip)

    def run(self):
        sock = socket(AF_INET, SOCK_DGRAM)
        try:
            sock.setsockopt(SOL_SOCKET, SO_BROADCAST, 1)
            global UDPqueue
            global msgs
            global addrs
            while running:
                if len(UDPqueue) != 0:
                    sock.sendto(UDPqueue[0].encode(), (self.ip, 8000))
                    # sock.sendto(UDPqueue[0].encode(), ("127.0.0.1", 8000))
                    print(UDPqueue[0])
                    UDPqueue.pop(0)
                else:
                    time.sleep(1)
        finally:
            sock.close()


# 메인 - 스레드 관리
if __name__ == '__main__':
    pr1 = UI()
    pr2 = UDPwrite()
    pr3 = UDPread()
    pr4 = TCP()
    pr1.setDaemon(True)
    pr2.setDaemon(True)
    pr3.setDaemon(True)
    pr4.setDaemon(True)
    pr1.start()
    pr2.start()
    pr3.start()
    pr4.start()
    while pr1.isAlive():
        running = True
    running = False
    pr1.join()
    pr2.join()
    pr3.stop()
    pr3.join()
    pr4.join()
