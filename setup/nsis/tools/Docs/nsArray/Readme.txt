nsArray NSIS plug-in

Author:   Stuart 'Afrow UK' Welch <afrowuk@afrowsoft.co.uk>
Company:  Afrow Soft Ltd
Language: C
Date:     2nd December 2014
Version:  1.1.1.7

Plug-in for NSIS which provides dynamic, indexed and associative arrays
(much like PHP). See Examples\nsArray\*.

------------------------------------------------------------------------
Features
------------------------------------------------------------------------

* Small DLL written in pure C
* Low memory usage
  - Linked lists
  - Allocates small blocks of memory rather than one contiguous block
* Much faster
  - Uses memory pointers
  - E.g. sorting copies memory pointers, not the data itself
* Very simple usage
  - Minimal set of functions
  - Dynamic array size
  - Indexed and associative (hashed)
* No limit on the number of arrays

------------------------------------------------------------------------
Notice
------------------------------------------------------------------------

  For plug-in call usage in this readme, square brackets ([, ]) around
  a plug-in call argument denote an optional argument.

  I.e. [...] means there can optionally be more repeated arguments of
  the same type already given.

------------------------------------------------------------------------
Setting & Getting Values
------------------------------------------------------------------------

  nsArray::Set my_array value

  Adds `value` to the end of `my_array` with the next element index as
  its key. If `my_array` does not already exist, it will be created.

  Example:
    ; array = {}
    nsArray::Set array hello
    ; array = {0 => hello}

  --------------------------------------------------------------------

  nsArray::Set my_array /key=key value

  Adds `value` at `key`, overwriting any existing value.

  Example:
    ; array = {}
    nsArray::Set array /key=Name Stuart
    ; array = {Name => Stuart}

  --------------------------------------------------------------------

  nsArray::Set my_array /at=position value

  Sets an element's value at the given zero-based position. An element
  must already exist at the given position, or else the call will fail.
  Specify a negative number to get an element starting from the end of
  the array, with -1 being the last element, -2 the second-to-last and
  so on. The error flag is set if the position is out of range.

  Example:
    ; array = {FirstName => Stuart, LastName => Welsh}
    nsArray::Set array /at=1 Welch
    ; array = {FirstName => Stuart, LastName => Welch}

  --------------------------------------------------------------------

  nsArray::SetList my_array element1 [element2 [...] /end

  Adds multiple elements to `my_array` (note that /end is required).
  Elements in the list are matched using the same syntax as the Set
  function (above).

  Example:
    ; array = {lol => 1}
    nsArray::SetList array /key=blah v1 v2 /at=0 v3 /key=9 22 v4 /end
    ; array = {lol => v3, blah => v1, 0 => v2, 9 => 22, 10 => v4}

  --------------------------------------------------------------------

  nsArray::Get my_array key
  Pop $value

  Gets the value from `my_array` with the given key (or index). The
  error flag is set if the array or key does not exist.

  Example:
    ; array = {0 => value1, blah => value2, 5 => value3}
    nsArray::Get array 0
    Pop $R0 ; value1
    nsArray::Get array blah
    Pop $R0 ; value2
    nsArray::Get array 5
    Pop $R0 ; value3

  --------------------------------------------------------------------

  nsArray::Get my_array /at=position
  Pop $key
  Pop $value

  Gets an element's key and value from `my_array` at the given
  zero-based position. This is useful for enumerating all keys and
  values. Specify a negative number to get an element starting from the
  end of the array, with -1 being the last element, -2 the second-to-
  last and so on. The error flag is set if the position is out of range.

  --------------------------------------------------------------------

  nsArray::Iterate my_array
  Pop $key
  Pop $value

  Gets the next element in the array for iteration through all elements,
  starting from the first element. The error flag is set when the
  iterator reaches the end of the array, on which the iterator is
  reset.

  --------------------------------------------------------------------

  nsArray::Iterate my_array /prev
  Pop $key
  Pop $value

  Gets the previous element in the array for iteration through all
  elements (i.e. in reverse order), starting from the last element. The
  error flag is set when the iterator reaches the end of the array,
  on which the iterator is reset.

  --------------------------------------------------------------------

  nsArray::Iterate my_array /reset

  Resets the current array iteration so that it can now start from the
  beginning of the array again.

------------------------------------------------------------------------
Removing Values
------------------------------------------------------------------------

  nsArray::Remove my_array element

  Removes a single element by key, position or value.

  Elements to remove can be matched using:

   key      - Remove an element with the given key (case sensitive).
   /at=pos  - Remove an element at the given zero-based position.
   /val=val - Remove an element with the given value (case insensitive).

  The error flag is set if the array does not exist or if the element
  cannot be removed.

  --------------------------------------------------------------------

  nsArray::RemoveList my_array element1 [element2 [...]] /end

  Removes multiple elements from `my_array` (note that /end is
  required). Elements in the list are matched using the same syntax as
  the Remove function (above).

  Example:
    ; array = {0 => ja, Hello => value1, blah => value2, 5 => value3}
    nsArray::Remove array 0
    ; array = {Hello => value1, blah => value2, 5 => value3}
    nsArray::RemoveList array Hello /val=VALUE2 /at=0 /end
    ; array = {}

  --------------------------------------------------------------------

  nsArray::Clear my_array

  Removes all elements from `my_array`. The error flag is set if the
  array does not exist.

------------------------------------------------------------------------
General Functions
------------------------------------------------------------------------

  nsArray::Sort my_array flags

  Sorts `my_array` using the given flags. Flags can be a combination of:

  1  = Sort in descending order rather than ascending.
  2  = Sort numeric rather than alpha.
  4  = Ignore character case.
  8  = Sort by keys rather than by values.
  16 = Do not reorder keys when sorting by values.

  E.g. specify `3` for `flags` to sort numbers in descending order. The
  error flag is set if the array does not exist.

  --------------------------------------------------------------------

  nsArray::Join my_array join_str [/noempty]
  Pop $joined

  Concatenates the values of `my_array` into a string with `join_str` as
  the delimiter. The error flag is set if the array does not exist or if
  the result cannot completely fit into the output buffer
  (NSIS_MAX_STRLEN) although as much as can fit into the output buffer
  will still be returned. Specify /noempty to skip empty entries.

  --------------------------------------------------------------------

  nsArray::Split my_array split_str delim_str [/noempty] [/ignorecase]

  Splits `split_str` into `my_array` using `delim_str` as the
  delimiter. Specify /noempty to skip empty entries. Specify
  /ignorecase to ignore the case of the delimiter.

  --------------------------------------------------------------------

  nsArray::Length my_array
  Pop $length

  Gets the number of elements in `my_array`. The error flag is set if
  the array does not exist.

------------------------------------------------------------------------
Script Header Functions
------------------------------------------------------------------------

  nsArray.nsh contains some useful functions to further extend the
  functionality of the plug-in. To use:

    !include nsArray.nsh

  --------------------------------------------------------------------

  ${nsArray_Copy} src_array dst_array

  Copies `src_array` to a new array called `dst_array`. The error flag
  is set if the source array does not exist. The destination array can
  already exist, in which case it is overwritten.

  --------------------------------------------------------------------

  ${nsArray_CopyKeys} src_array dst_array

  Copies the keys from `src_array` to a new array called `dst_array`.
  The error flag is set if the source array does not exist. The
  destination array can already exist, in which case it is overwritten.

  --------------------------------------------------------------------

  ${nsArray_ToString} my_array
  Pop $var

  Returns a string representation of `my_array` which includes keys and
  values in the format key1 => value1, key2 => value2, etc.. The error
  flag is set if the array does not exist or if the result cannot
  completely fit into the output buffer (NSIS_MAX_STRLEN) although as
  much as can fit into the output buffer will still be returned.

  --------------------------------------------------------------------

  ${ForEachIn} my_array $key $value
    DetailPrint "my_array[$key] => $value"
  ${Next}

  An array implementation of the LogicLib ForEach loop. The loop will
  iterate over each element of the array, where $key and $value are an
  element's respective key and value. One can iterate in reverse order
  using ${ForEachInReverse}.

------------------------------------------------------------------------
Change Log
------------------------------------------------------------------------

1.1.1.7 - 2nd December 2014
* x64 ANSI/Unicode builds.
* DLL is no longer UPX compressed.

1.1.1.6 - 1st June 2013
* Fixed memory access violation in Iterate function due to NULL pointer
  when an invalid array name is supplied.

1.1.1.5 - 26th May 2013
* Added /at= for Set and SetList functions.
* Fixed SetList and RemoveList not setting the error flag on error if
  one element was not set/added or removed successfully.
* Added Iterate function which replaces iteration using Get with /next,
  /prev and /reset.

1.1.1.4 - 3rd September 2012
* Replaced Set /list with SetList.
* Replaced Remove /list with RemoveList.

1.1.1.3 - 20th March 2012
* Set and Remove functions require /list and /end when specifying
  multiple elements to avoid eating up the stack. /end is not necessary
  unless /list was used.
* Fixed Remove function eating up the stack if the array does not exist.

1.1.1.2 - 13th February 2012
* Replaced /at=-1 with /next for Get function.
* Replaced /at=-2 with /prev for Get function.
* Added /reset switch to Get function to reset /next or /prev iteration.
* The /at=position switch for Get and Remove functions now support
  negative numbers.
* Added call to Get /reset before entering ForEachIn loop.
* Fixed incorrect usage of Split function in the readme.

1.1.1.1 - 12th February 2012
* Optimised Get /at=-1 by using a pointer instead of a counter.
* Added Get /at=-2 to iterate in reverse order.
* Added script header ForEachInReverse LogicLib loop.

1.1.1.0 - 12th February 2012
* Added Split function.
* Added /noempty switch to Join function.
* Sort function now supports ORing (|) for its sort flags.
* Replaced Delete function with Clear function.
* Replace GetAt function with /at=position switch on Get function.
* Added Get /at=-1 for array element iteration.
* Added /at=position and /val=value switches for Remove function.
* Implemented script header replacements for Copy, CopyKeys and
  ToString functions.
* Added script header ForEachIn LogicLib loop.

1.1.0.2 - 12th October 2011
* Added GetAt function.
* Added additional sort flag (16) to retain keys/indices order when
  sorting values.
* Removed New function. Set function now creates the array if it doesn't
  already exist.

1.1.0.1 - 9th September 2011
* Fixed Get function returning incorrect values.

1.1.0.0 - 2nd July 2011
* Re-designed many plug-in functions to deal with indexed and
  associative arrays.
* Added ToString function.
* Added CopyKeys function.
* Added additional Sort function flag (8).

1.0.0.1 - 2nd June 2011
* Fixed decrementing of element and array counters on element and array
  deletion.
* Fixed memory leak of array name on array deletion.
* Fixed bad pointers remaining after last element and array deletion.

1.0.0.0 - 7th May 2011
* First version.

------------------------------------------------------------------------
License
------------------------------------------------------------------------

This plug-in is provided 'as-is', without any express or implied
warranty. In no event will the author be held liable for any damages
arising from the use of this plug-in.

Permission is granted to anyone to use this plug-in for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this plug-in must not be misrepresented; you must not
   claim that you wrote the original plug-in.
   If you use this plug-in in a product, an acknowledgment in the
   product documentation would be appreciated but is not required.
2. Altered versions must be plainly marked as such, and must not be
   misrepresented as being the original plug-in.
3. This notice may not be removed or altered from any distribution.