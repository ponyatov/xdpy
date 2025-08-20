int hello(void *ctx) {
    bpf_trace_printk("Hello world! clone() is calling\n");
    return 0;
}
