using System;
using System.Collections.Generic;
using System.Text;

namespace DevTools
{
    public static class Help
    {
        public static string DEFAULTFUNCS = @"SYSTEM FUNCTIONS:" + "\n"
                                          + @"loop(i)///Does a loop thingy<new>Specify the amount of times you want to loop in the brackets.<new>Put 'i' everywhere that you want the int of the amount of iterations to be<new>For example, *loop(10):asci(i)*<new>This will print in a cool asci font the numbers from 0,9\\\" + "\n"
                                          + @"#define()///*#define bob = 2* will replace everywhere in any line of code you ever write again where you put *bob*, and change it to *2*<new>For example, *ran(bob,5)* will change to *ran(2,5)*\\\" + "\n"
                                          + @"#defunc()///Allows you to define a function with paramaters.<new>For example, *#defunc bob(x)asci(ran(0,x))*<new>This will print out a random number between 0,the inputted variable.<new>Everywhere you type in *x* or whatever the variable name specified is, will be replaced with the users input.<new>This can be called like *bob(5)*, and will replace everywhere you put *x* with *5*\\\" + "\n"
                                          + @"#delfunc()///Allows you to delete a defined function.<new>For example *#delfunc bob* will delete the function bob\\\" + "\n"
                                          + @"#del()///Allows you to delete variables.<new>For example, if you already had a *#define bob = 2* variable, it could be deleted with:<new>*#del bob*\\\" + "\n"
                                          + @"nw()///Type *nw* before a command, and it will not show any working out for the calculation.<new>For example, *nw 100+100+100* will not print out math workings for the equation\\\" + "\n"
                                          + @"showfunc()///Type in plain text, with no other functions *showfunc* and it will show you all the user defined functions that you have\\\" + "\n"
                                          + @"dv()///Type in plain text, with no other functions *dv* and it will show you all the *#define*'s that you have.\\\" + "\n"
                                          + @"dtv()///Type in plain text, with no other functions *dtv* and it will show you all the *var*'s that you have.\\\" + "\n"
                                          + @"exit()///Closes the application\\\" + "\n"
                                          + @"quit()///Closes the application\\\" + "\n"
                                          + @"alg()///Can be used to factorise an algebraic expression.<new>For example, the equation *6x² - 11x - 7* can be solved with *alg(6,-11,-7)<new>This is a standalone function. Does not work with others*\\\" + "\n"
                                          + @"v()///After you type in a command, for example *100+100*, whatever the result is will be replaced with *v*<new>For example, if your next line of code was *7+v*, this would print out *207*<new>v is y default put at the start of a statement.<new>For example, if you just typed *<<2*, that would be replaced by *v<<2*, which would be replaced with *207<<2*\\\" + "\n"
                                          + @"doub()///Shows a value as a double, including binary output.<new>For example, *doum 6.7764* will show you how a double value of *6.7764* is stored in binary\\\" + "\n"
                                          + @"float()///Shows a value as a float, including binary output.<new>For example, *float 6.7764* will show you how a float value of *6.7764* is stored in binary\\\" + "\n"
                                          + @"adv()///Shows whatever the current value of *v* is in a double binary output.<new>Not to be used in conjunction with other functions\\\" + "\n"
                                          + @"afv()///Shows whatever the current value of *v* is in a float binary output.<new>Not to be used in conjunction with other functions\\\" + "\n"
                                          + @"dt()///Prints out the current date & time.<new>Not to be used in conjunction with other functions\\\" + "\n"
                                          + @"var()///Similiar to *#define*, except variable defined is only temporary.<new>i.e. after you close and open the app, the variable will be deleted<new>Can be used like this: *var bob = 2*\\\" + "\n"
                                          + @"np()///Add at the start of a statement, to not print out a binary output<new>For example, *np 2+2* will print out the answer to the equation, *4*, but not its binary value\\\" + "\n"
                                          + @"hrgb()///Converts a hex colour value to rgb.<new>Can be used like this: *hrgb ffffff*<new>DO NOT USE *#*'s when defining. These will be converted into a ulong, which then gets converted back to hex<new>This will of course result in you getting an inaccurate result\\\" + "\n"
                                          + @"asci()///Prints out text, or a number specified in the brackets in a cool ascii font<new>Can be used like: *asci(bob)*<new>However, since when you type in a command, all spaces are removed, you can specify that you want spaces like this: *asci("+"\""+"bob owns a dog"+"\""+ @")*<new>If in this example, you already had a variable defined as *bob*, its value would be replaced with the text 'bob'\\\" + "\n"
                                          + @"basci()///Prints out a number, specified in the brackets in a binary ascii font. Decimal points also allowed<new>Can be implemented like this: *basci(3.14159)*<new>CANNOT PRINT OUT PLAIN TEXT\\\" + "\n"
                                          + @"pw()///Changes the default value for not printing workings.<new>Can specify value in brackets<new>For example: *pw(false)* will disable printing workings for current session\\\" + "\n"
                                          + @"fpw()///Same as pw, except instead of only saving the value for the current session, it saves it in a file, so that it is like this permanently.<new>To see how to use *pw*, type *help-pw*\\\" + "\n"
                                          + @"cv()///Clears all variables. Both temporary and *#define* type variables\\\" + "\n"
                                          + @"avg()///Gets the average from a set of numbers.<new>For example: *avg(10,20,30,40)* gives an average of 25<new>Can include decimals\\\" + "\n"
                                          + @"f()///Prints out binary value flipped horizontally\\\" + "\n"
                                          + @"rf()///Changes the default *f* value. Saves only for the current session\\\" + "\n"
                                          + @"ati()///Just type random letters between the brackets. You'll figure it out, or you wont<new>Used like this: *ati(WASSUP)*\\\<new>I legitemately have no idea why I added this" + "\n"
                                          + @"i()///Show the value in binary as a *32 bit uint*<new>Used like this: *i255*\\\" + "\n"
                                          + @"s()///Show the value in binary as a *16 bit ushort*<new>Used like this: *s255*\\\" + "\n"
                                          + @"b()///Show the value in binary as a *8 bit byte*<new>Used like this: *b255*\\\" + "\n"
                                          + @"h()///Show the value in hexadecimal<new>Used like this: *h255*\\\" + "\n"
                                          + @"#_()///Show the hex value as a ulong<new>Used like this: *#_ffffff*\\\" + "\n"
                                          + @"b_()///Show the binary value as a ulong<new>Used like this: *#_10110010*\\\" + "\n"
                                          + @"doum()///Does decimal math on an inputted equation<new>Cannot do bitmath, as value is stored in a double\\\" + "\n"
                                          + @"booleans()///Function used to determine if a certain rule is true<new>For example, *4==4?3:2* will return *3* if *4* is in fact equal to *4* (u figure it out)<new>This can also be done with functions.<new>For example, *4==4?asci("+"\"four is equal to four\"):asci(\"hol up\")*"+@"<new>If a *:* is not included, if the value is *false*, *0* will be returned<new>You can also put in singular binary operations, such as *5>2*.<new>This will print out true, if *5* is in fact larger than *2* (research is being done)<new>Allowed conditionary operators are: *<*, *>*, *==*, *!=*\\\" + "\n"
                                          + @"bitmath()///<< will shift the current binary values *1*'s to the left.<new>>> will do the same, except shift to the right<new>| will do a bitor on the values, for example *4|5*<new>Similiarly to the bitor, *^* does an exor, and *&* does a bitand<new>If you don't understand binary, don't bother using these\\\" + "\n"
                                          + @"trig()///You can use trig functions such as *cos, arccos, tan, arctan, sin, arcsin* to do trig operations.<new>They are all used in the same format<new>They are used like this: *sin(90)*<new>This automatically does a *doum* operation, so you can use other math functions with it as well\\\" + "\n"
                                          + @"log()///You can do logarithimic functions specifying a base and a number<new>For example, *log8(10)* this has a base of *8*, and a num of *10*<new>If no base is specified, default is *10*<new>This automatically does a *doum* operation, so you can use other math functions with it as well\\\" + "\n"
                                          + @"ran()///Generates a random number, specify lower an upper bounds like so: *ran(lower,upper)*<new>Example usage: *ran(1,5)*\\\" + "\n"
                                          + @"ipconfig()///Type *ipconfig* in plain text to veiw networking data.<new>Similiar to *ipconfig* in cmd\\\" + "\n"
            ;
    }
}