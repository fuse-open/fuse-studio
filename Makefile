default:
	@bash scripts/build.sh
fast:
	@bash scripts/build.sh --fast
all:
	@bash scripts/build.sh --install
kill:
	@bash scripts/kill.sh
run:
	@npm run -s fuse
daemon:
	@npm run -s daemon
clean:
	@npm run -s clean
nuke:
	@npm run -s nuke
install:
	@npm link -f
uninstall:
	@bash setup/uninstall.sh
pack:
	@bash scripts/prepack.sh
	@npm pack
	@bash scripts/postpack.sh
setup:
	@bash setup/build.sh
assets:
	@bash assets/sync.sh
apk:
	@npm run -s app:build-apk
aab:
	@npm run -s app:build-aab

.PHONY: setup assets
