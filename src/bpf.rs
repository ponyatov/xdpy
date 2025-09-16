//! eBPF backend using aya library
//! - https://github.com/austinhartzheim/bpf-experiments/blob/master/kernelspace/src/probe/main.rs

#![no_std]
#![no_main]

use aya_ebpf::{
    bindings::{XDP_DROP, XDP_PASS},
    macros::xdp,
    programs::XdpContext,
};

#[xdp]
pub fn xdp_program(ctx: XdpContext) -> u32 {
    match process_packet(&ctx) {
        Ok(action) => action,
        Err(_) => XDP_DROP,
    }
}

fn process_packet(_ctx: &XdpContext) -> Result<u32, ()> {
    // Packet processing logic here
    Ok(XDP_PASS)
}

#[panic_handler]
fn panic(_info: &core::panic::PanicInfo) -> ! {
    unsafe { core::hint::unreachable_unchecked() }
}
