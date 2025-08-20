.PHONY: py
py:     requirements.txt $(PY) $(PIP)
	$(PIP) install -U -r $<
$(PY) $(PIP):  pyvenv.cfg
pyvenv.cfg:
	python3 -m venv .
