#!/usr/bin/env python3

## https://habr.com/ru/articles/683566/

from bcc import BPF

BPF_PROGRAM = r"""
int hello(void *ctx) {
  bpf_trace_printk("Hello world! clone() is calling\n");
  return 0;
}
"""

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
