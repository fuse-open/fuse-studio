#include <stdlib.h>
#include <string.h>

#import <Cocoa/Cocoa.h>

//#define MONO_LIB_PATH(lib) "/Library/Frameworks/Mono.framework/Versions/Current/lib/"lib

static int
check_mono_version (const char *version, const char *req_version)
{
	char *req_end, *end;
	long req_val, val;

	while (*req_version) {
		req_val = strtol (req_version, &req_end, 10);
		if (req_version == req_end || (*req_end && *req_end != '.')) {
			fprintf (stderr, "Bad version requirement string '%s'\n", req_end);
			return FALSE;
		}

		req_version = req_end;
		if (*req_version)
			req_version++;

		val = strtol (version, &end, 10);
		if (version == end || val < req_val)
			return FALSE;

		if (val > req_val)
			return TRUE;

		if (*req_version == '.' && *end != '.')
			return FALSE;

		version = end + 1;
	}

	return TRUE;
}

static char *
str_append (const char *base, const char *append)
{
	size_t baselen = strlen (base);
	size_t len = strlen (append);
	char *buf;

	if (!(buf = malloc (baselen + len + 1)))
		return NULL;

	memcpy (buf, base, baselen);
	strcpy (buf + baselen, append);

	return buf;
}

static char *
generate_fallback_path (const char *monoDir)
{
	char *lib_dir;
	char *result;

	/* Inject our Resources/lib dir */
	lib_dir = str_append (monoDir, "/lib:");

	if (lib_dir == NULL)
		abort ();

	/* Mono's lib dir, and CommandLineTool's lib dir into the DYLD_FALLBACK_LIBRARY_PATH */
	result = str_append (lib_dir, "/Library/Frameworks/Mono.framework/Versions/Current/lib:/lib:/usr/lib:/Library/Developer/CommandLineTools/usr/lib:/usr/local/lib");

	free (lib_dir);
	return result;
}

static bool
env2bool (const char *env, bool defaultValue)
{
	const char *value;
	bool nz = NO;
	int i;

	if (!(value = getenv (env)))
		return defaultValue;

	if (!strcasecmp (value, "true"))
		return YES;

	if (!strcasecmp (value, "yes"))
		return YES;

	/* check to see if the value is numeric. All numeric values evaluate to true *except* zero */
	for (i = 0; value[i]; i++) {
		if (!isdigit ((int) ((unsigned char) value[i])))
			return NO;

		if (value[i] != '0')
			nz = YES;
	}

	return nz;
}

static bool
push_env (const char *variable, const char *value)
{
	const char *current;
	size_t len;
	char *buf;
	BOOL updated = YES;

	if ((current = getenv (variable)) && *current) {
		char *token, *copy;

		copy = strdup (current);
		len = strlen (value);
		while ((token = strsep(&copy, ":"))) {
			if (!strncmp (token, value, len)) {
				while ((strsep(&copy, ":")))
					continue;

				updated = NO;
				goto done;
			}
		}

		if (!(buf = malloc (len + strlen (current) + 2)))
			return NO;

		memcpy (buf, value, len);
		buf[len] = ':';
		strcpy (buf + len + 1, current);
		setenv (variable, buf, 1);
		free (buf);
done:
		free (copy);
	} else {
		setenv (variable, value, 1);
	}

	//if (updated)
	//	printf ("Updated the %s environment variable with '%s'.\n", variable, value);

	return updated;
}

static bool
update_environment (const char *monoPath)
{
	bool updated = NO;
	char *value;

	if ((value = generate_fallback_path (monoPath))) {
		char *token;

		while ((token = strsep(&value, ":"))) {
			if (push_env ("DYLD_FALLBACK_LIBRARY_PATH", token))
				updated = YES;
		}

		free (value);
	}

	if((value = str_append(monoPath, "/etc"))) {
		setenv("MONO_CFG_DIR", value, 1);
		free(value);
	}

	if((value = str_append(monoPath, "/lib/mono/4.5"))) {
		if(push_env("MONO_PATH", value))
			updated = YES;

		free(value);
	}

	if((value = str_append(monoPath, "/lib/mono/4.5/Facades"))) {
		if(push_env("MONO_PATH", value))
			updated = YES;

		free(value);
	}

	if((value = str_append(monoPath, "/etc/fonts"))) {
		setenv("FONTCONFIG_PATH", value, 1);

		free(value);
	}

	return updated;
}