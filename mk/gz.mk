GZ += static/cdn/jquery.min.js
static/cdn/jquery.min.js:
	$(CURL) $@ https://code.jquery.com/jquery-$(JQUERY_VER).min.js
