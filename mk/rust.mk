RTARGET = x86_64-unknown-linux-gnu

$(RUSTUP) $(CARGO):
	curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
	rustup target add x86_64-unknown-linux-gnu
	rustup target add wasm32-unknown-unknown
	rustup component add rust-analyzer rustfmt
	cargo install cargo-watch cargo-binutils

.PHONY: recv
recv: $(R)
	cargo watch -x "run --bin $@ --features=pc --target x86_64-unknown-linux-gnu -- $(S)"

.PHONY: send
send: $(R)
	cargo watch -x "run --bin $@ --features=pc --target x86_64-unknown-linux-gnu -- $(S)"

.PHONY: bpfel
# https://aya-rs.dev/book/start/development/#prerequisites
# 	rustup target add bpfel-unknown-none
bpfel:
	rustup install stable
	rustup toolchain install nightly --component rust-src
	cargo install bpf-linker
