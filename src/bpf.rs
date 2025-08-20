//! eBPF backend

#![no_std]
#![no_main]

// Required export for some BPF loaders
#[no_mangle]
pub extern "C" fn _license() -> *const u8 {
    b"GPL\0".as_ptr()
}

#[panic_handler]
fn panic(_info: &PanicInfo) -> ! {
    loop {}
}

#[alloc_error_handler]
fn alloc_error(_layout: core::alloc::Layout) -> ! {
    loop {}
}

// BPF program entry point (XDP example)
#[no_mangle]
pub extern "C" fn xdp_program(ctx: *mut u8) -> i32 {
    // Your XDP program logic here
    // Return XDP_PASS, XDP_DROP, etc.
    2 // XDP_PASS
}

// Alternative: Traffic control program
#[no_mangle]
pub extern "C" fn tc_program(ctx: *mut u8) -> i32 {
    // TC program logic
    0 // TC_ACT_OK
}
