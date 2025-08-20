# XDP
## [[eBPF]]

## [[xdpy/debug]]

## [[Mellanox]] cards

The [Mellanox ConnectX NIC](https://www.mellanox.com/products/ethernet/connectx-smartnic) family allows metadata to be prepared by the [[net/NIC|NIC]] hardware. This metadata can be used to perform hardware acceleration for applications that use XDP.

- https://developer.nvidia.com/blog/accelerating-with-xdp-over-mellanox-connectx-nics/

Because XDP runs in the kernel network driver, it can read the ethernet frames from the RX ring of the [[net/NIC]] and take actions immediately.

![[XDP_DROP]]

```sh
sudo sysctl net/core/bpf_jit_enable
```
```
net.core.bpf_jit_enable = 1
```

## tools

- [[xdp-trafficgen]]

## API

- [Создание XDP eBPF программы с использованием C и Golang](https://habr.com/ru/companies/otus/articles/860104/)
