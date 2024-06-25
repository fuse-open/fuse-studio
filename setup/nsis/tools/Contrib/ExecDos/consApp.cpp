#include <windows.h>
#include <stdio.h>

#pragma comment(linker,"/MERGE:.rdata=.text")
#pragma comment(linker,"/FILEALIGN:512 /SECTION:.text,EWRX /IGNORE:4078")
#pragma comment(linker,"/ENTRY:main")
#pragma comment(linker,"/NODEFAULTLIB")

int main( int argc, char **argv)
{
   char b[1024] = "";
   printf("This is test console application for execDos plugin\n");
   printf("Written by Takhir Bedertdinov\n\n");
   fflush(NULL);
   Sleep(500);
   printf("Login: ");
   fflush(NULL);
   Sleep(500);
//   fgets(b, sizeof(b), stdin);// generates error in msvcrt
   gets(b);
   printf("%s\n", b);
/*MessageBox(NULL, "Login received", b, 0);*/
   fflush(NULL);
   Sleep(500);
   printf("Passowrd: ");
   fflush(NULL);
   Sleep(500);
   *b = 0;
//   fgets(b, sizeof(b), stdin);
   gets(b);
   printf("%s\n", b);
   fflush(NULL);
   Sleep(500);
/*MessageBox(NULL, "Password received", b, 0);*/
   printf("\nThanks\nBla-bla-bla\nBye\n");
   fflush(NULL);
   Sleep(500);
	return 5;
}

