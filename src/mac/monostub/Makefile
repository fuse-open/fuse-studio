all: monostub monostub-console

monostub: monostub.m
	@clang -m64 monostub.m -o monostub -framework AppKit -mmacosx-version-min=10.10

monostub-console: monostub-console.m
	@clang -m64 monostub-console.m -o monostub-console -framework AppKit -mmacosx-version-min=10.10
