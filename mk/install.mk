.PHONY : install update ref gz
install: $(WS)_install $(PIP) $(RUSTUP) doc ref gz
	$(MAKE) update
update : $(WS)_update  $(PIP) $(RUSTUP)
	$(RUSTUP) self update && $(RUSTUP) update
	$(PIP) install -U    pip
	$(PIP) install -U -r requirements.txt
ref    : $(REF)
gz     : $(GZ)

Debian_install:
# sudo dpkg --add-architecture i386
Debian_update:
	sudo apt update
	sudo apt install -uy `cat apt.$(WS)` $(APT)
	$(PIP) install -U    pip
	$(PIP) install -U -r requirements.txt
