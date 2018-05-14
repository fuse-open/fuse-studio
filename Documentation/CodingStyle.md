# Coding Style

Please consider using an [EditorConfig](http://editorconfig.org/)-capable
text editor. Plug-ins exist for most popular text editors. This ensures
that our [configuration file](../.editorconfig) gets respected, which
reduces needless commit-noise.

Generally, try to follow the local code style of the source file you're
editing. It's much more disturbing to switch between multiple code styles
within a source file, than between source files.

These guidelines apply only to code written and maintained as a part of
this project. Any code that gets imported from some upstream source
should keep the style from its upstream. This ensures it's as easy as
possible to apply patches from the upstream source.

## General

We generally try to write idiomatic code for each language we use. In
addition, we have some language-specific guidelines (listed below).

Do not commit commented-out code. Commented-out code is harder to
maintain, as it's not actively compiled, and usually turns out being
nothing more than noise. If something is really important to keep around,
consider keeping it on a private branch, or as code that still gets
compiled, but not executed. Do note that dead code can easily get removed
by someone else.

Avoid needless whitespace changes. Unnecessary whitespace changes lead
to hard to backport patches, and increase the chance of merge conflicts.
Please avoid them. They also make it harder to review a pull request. If
a file is whitespace broken, consider putting the whitespace cleanup in
a separate commit, so it's easier to omit it when reviewing.

## Uno / C&#35;

We generally use [idiomatic C# code style](https://msdn.microsoft.com/en-us/library/ff926074.aspx),
but we also have a few additional guidelines:

* Use one tab for indentation
* Non-public members are prefixed with an underscore, like so:
   ```csharp
   class Foo
   {
   	int _bar;
   }
   ```
* *Do not* use Egyptian braces
* Use spaces around operators and keywords, but not inside parentheses and
  not around function calls. So, like this:
  ```csharp
  if (condition)
        foo(bar + 1);
  ```
  ...and not like this:
  ```csharp
  if( condition )
        foo( bar+1 );
  ```
