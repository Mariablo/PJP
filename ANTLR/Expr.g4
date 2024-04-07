// GRAMMAR NAME - Grammar
grammar Expr;

program   :   command * EOF                                ;

command   :   '{' command* '}'                              # Code_block 
          |   primitiveType IDENTIFIER (',' IDENTIFIER)* ';'   # Declaration
          |   IF '(' expr ')' command (ELSE command)?      # If
          |   WHILE '(' expr ')' command                   # While
          |   expr ';'                                     # Expression
          |   READ IDENTIFIER (',' IDENTIFIER)* ';'        # Read
          |   WRITE expr (',' expr)* ';'                   # Write
          |   ';'                                          # Empty;


expr      
    : primary
    | prefix='-' expr
    | prefix='!' expr
    | expr op=('*'|'/'|'%') expr
    | expr op=('+'|'-'|'.') expr
    | expr op=('>' | '<')   expr
    | expr op=('=='|'!=')   expr
    | expr op='.'           expr
    | expr op='&&'          expr
    | expr op='||'          expr
    | <assoc=right> IDENTIFIER '=' expr
    ;


primary
    : '(' expr ')'
    | DECIMAL_LITERAL
    | FLOAT_LITERAL
    | STRING_LITERAL
    | BOOL_LITERAL
    | IDENTIFIER
    ;


// LITERALS
// DATA TYPES KEYWORDS

primitiveType
    : BOOL
    | INT
    | FLOAT
    | STRING
    ;

BOOL:	'bool';
INT:	'int';
FLOAT:	'float';
STRING:	'string';
IF:		'if';
ELSE:	'else';
WHILE:	'while';
READ:	'read';
WRITE:	'write';


// Literals

DECIMAL_LITERAL:	[0-9]+;
FLOAT_LITERAL:		[0-9]+'.'[0-9]+;

BOOL_LITERAL
	: 'true'
	| 'false'
	;

STRING_LITERAL: '"' (~["\\\r\n])* '"';


// Separators

LPAREN:	'(';
RPAREN:	')';
LBRACE:	'{';
RBRACE:	'}';
LBRACK: '[';
RBRACK: ']';
SEMI:	';';
COMMA:	',';

// Operators

DOT:				'.';
ASSIGN:				'=';
GT:					'>';
LT:					'<';
BANG:				'!';
EQUAL:				'==';
NOTEQUAL:			'!=';
AND:				'&&';
OR:					'||';
ADD:				'+';
SUB:				'-';
MUL:				'*';
DIV:				'/';
MOD:				'%';


// Other

WS:					[ \t\r\n\u000C]+	-> channel(HIDDEN);
LINE_COMMENT:		'//' ~[\r\n]*		-> channel(HIDDEN);

// Identifiers

IDENTIFIER:			Letter LetterOrDigit*;

// Fragment rules

fragment LetterOrDigit
	: Letter
	| [0-9]
	;

fragment Letter
	: [a-zA-Z$_]
	;