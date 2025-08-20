# veth
## Virtual [[Ethernet]] Interface

This is the most common and realistic method for debug. It creates a virtual [[Ethernet]] pair (like a virtual network cable), allowing you to load your [[XDP]] program on one end and generate traffic from the other.

- **Create the [[veth]] pair:**
```sh
sudo ip link add veth0 type veth peer name veth1
```
```sh
ip addr
```
```
6: veth1@veth0: <BROADCAST,MULTICAST,M-DOWN> mtu 1500 qdisc noop state DOWN group default qlen 1000
    link/ether aa:2a:38:2d:3e:9f brd ff:ff:ff:ff:ff:ff
7: veth0@veth1: <BROADCAST,MULTICAST,M-DOWN> mtu 1500 qdisc noop state DOWN group default qlen 1000
    link/ether 66:a6:b7:52:bf:8c brd ff:ff:ff:ff:ff:ff
```

- **Assign IP addresses and bring them up:**
```sh
sudo ip addr add 111.111.111.111/24 dev veth0
sudo ip addr add 222.222.222.222/24 dev veth1
sudo ip link set veth0 up
sudo ip link set veth1 up
```
```
6: veth1@veth0: <BROADCAST,MULTICAST,UP,LOWER_UP> mtu 1500 qdisc noqueue state UP group default qlen 1000
    link/ether aa:2a:38:2d:3e:9f brd ff:ff:ff:ff:ff:ff
    inet 222.222.222.222/24 scope global veth1
       valid_lft forever preferred_lft forever
    inet6 fe80::a82a:38ff:fe2d:3e9f/64 scope link
       valid_lft forever preferred_lft forever
7: veth0@veth1: <BROADCAST,MULTICAST,UP,LOWER_UP> mtu 1500 qdisc noqueue state UP group default qlen 1000
    link/ether 66:a6:b7:52:bf:8c brd ff:ff:ff:ff:ff:ff
    inet 111.111.111.111/24 scope global veth0
       valid_lft forever preferred_lft forever
    inet6 fe80::64a6:b7ff:fe52:bf8c/64 scope link
       valid_lft forever preferred_lft forever
```

## network/interfaces.d

To add a [[veth]] pair permanently on a system using `/etc/network/interfaces.d/`, you'll need to create a configuration file.

```sh
sudo vim /etc/network/interfaces.d/veth.conf
```
```
# Virtual Ethernet pair for XDP testing
auto veth0
iface veth0 inet manual
    pre-up ip link add veth0 type veth peer name veth1
    up ip link set veth1 up
    up ip addr add 111.111.111.111/24 dev veth0
    up ip addr add 111.111.111.222/24 dev veth1
    up ip link set veth0 up
    down ip link del veth0
```
