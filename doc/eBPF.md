# eBPF
## Extended Berkeley Packet Filter

https://ebpf.io/

## classic BPF

[[#classic BPF]] defines a type of packet filter, implemented as a virtual machine, that can run in the [[Linux/kernel]]. Before BPF, packet filters ran only in user space, which was much more CPU-intensive than kernel-level filtering. BPF has typically been used for programs that need to capture and analyze packets efficiently. It is what allows [[tcpdump]], for example, to filter out irrelevant packets very quickly.

The [[eBPF]] extension to BPF was created for top-level network traffic processing. Lets you add hooks to kernel system calls (syscalls) and functions, including network-related functions, to provide visibility into traffic payloads and function results (success/failure).

## [[XDP]]

- https://github.com/mikeroyal/eBPF-Guide

## [[Python]]

```sh
sudo apt install -uy bpftool bpfcc-tools python3-bpfcc
```
