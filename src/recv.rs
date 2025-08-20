mod config;

fn main() {
    let argv: Vec<String> = std::env::args().collect();
    let _argc = argv.len();
    arg(0, &argv[0]);
    for (i, argv) in argv.iter().enumerate().skip(1) {
        arg(i, argv);
    }
}

fn arg(argc: usize, argv: &str) {
    eprintln!("argv[{argc}] = {argv:?}");
}
