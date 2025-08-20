#!/usr/bin/env python3

## https://habr.com/ru/articles/683566/

from bcc import BPF

with open('src/hello.bpf.c') as bc:
    BPF_PROGRAM = bc.read()

b = BPF(text=BPF_PROGRAM)
clone = b.get_syscall_fnname("clone")
b.attach_kprobe(event=clone, fn_name="hello")

while True:
    try:
        (task, pid, cpu, flags, ts, msg) = b.trace_fields()
    except ValueError:
        continue
    except KeyboardInterrupt:
        break
    print(f"{ts} {pid}/{cpu} {flags} {task.decode()} {msg.decode()}")
