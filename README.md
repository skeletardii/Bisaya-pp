# Language Specification: BISAYA++ 

## Introduction

**Bisaya++** is a strongly–typed high–level interpreted Cebuano-based programming language developed to teach Cebuanos the basics of programming. Its simple syntax and native keywords make programming easy to learn.

### Sample Program

```bisaya
-- this is a sample program in Bisaya++
SUGOD

MUGNA NUMERO x, y, z=5  
MUGNA LETRA a_1=’n’  
MUGNA TINUOD t=”OO”  

x=y=4  
a_1=’c’  
-- this is a comment  

IPAKITA: x & t & z & $ & a_1 & [#] & “last”  
KATAPUSAN
```

#### Output

```
4OO5
c#last
```

## Language Grammar

### Program Structure

- All code must be inside `SUGOD` and `KATAPUSAN`.
- Variable declarations begin with `MUGNA`.
- Variable names are case sensitive and must start with a letter or underscore (`_`), followed by letters, digits, or underscores.
- One statement per line.
- Comments start with `--`.
- All reserved words are capitalized and cannot be used as identifiers.
- `$` is used to indicate a newline.
- `&` is used for concatenation.
- Square brackets `[]` are used for escape codes.

## Data Types

| Type     | Description                        |
|----------|------------------------------------|
| `NUMERO` | Whole numbers (4 bytes)            |
| `LETRA`  | Single character                   |
| `TINUOD` | Boolean literal: `OO` or `DILI`    |
| `TIPIK`  | Numbers with decimal part          |

## Operators

### Arithmetic

| Symbol | Meaning                  |
|--------|--------------------------|
| `*`    | Multiplication            |
| `/`    | Division                  |
| `%`    | Modulo                    |
| `+`    | Addition                  |
| `-`    | Subtraction               |
| `()`   | Grouping with parenthesis |

### Relational

| Symbol | Meaning                        |
|--------|--------------------------------|
| `>`    | Greater than                   |
| `<`    | Less than                      |
| `>=`   | Greater than or equal to       |
| `<=`   | Less than or equal to          |
| `==`   | Equal                          |
| `<>`   | Not equal                      |

### Logical

| Keyword | Meaning                                |
|---------|----------------------------------------|
| `UG`    | AND – both expressions must be true    |
| `O`     | OR – at least one must be true         |
| `DILI`  | NOT – reverses the Boolean expression  |

### Boolean Literals

- `"OO"` – TRUE  
- `"DILI"` – FALSE

### Unary

- `+` – Positive  
- `-` – Negative

## I/O Statements

### Output

- `IPAKITA:` – Displays formatted output.

### Input

- `DAWAT:` – Reads input values.  
  **Syntax**: `DAWAT: <var1>[, <var2>, ...]`  
  **Example**: `DAWAT: x, y` (User inputs two values separated by commas)

## Control Flow

### Conditional

#### `KUNG` (If)

```bisaya
KUNG (<BOOL expression>) 
PUNDOK{
  <statements>
}
```

#### `KUNG-KUNG WALA` (If-Else)

```bisaya
KUNG (<BOOL expression>) 
PUNDOK{
  <statements>
}
KUNG WALA
PUNDOK{
  <statements>
}
```

#### `KUNG-KUNG DILI` (If-Else If-Else)

```bisaya
KUNG (<BOOL expression>) 
PUNDOK{
  <statements>
}
KUNG DILI (<BOOL expression>) 
PUNDOK{
  <statements>
}
KUNG WALA
PUNDOK{
  <statements>
}
```

> **Note:** `PUNDOK{}` is used to group multiple statements.

### Looping

#### `ALANG SA` (For Loop)

```bisaya
ALANG SA (<init>, <condition>, <update>)
PUNDOK{
  <statements>
}
```

**Example:**

```bisaya
ALANG SA (ctr=1, ctr<=10, ctr++)
PUNDOK{
  IPAKITA: ctr & ' '
}
```

**Output:**

```
1 2 3 4 5 6 7 8 9 10
```

## Example Programs

### Arithmetic Example

```bisaya
SUGOD
MUGNA NUMERO xyz, abc=100  
xyz= ((abc * 5) / 10 + 10) * -1  
IPAKITA: [[] & xyz & []]  
KATAPUSAN
```

**Output:**

```
[-60]
```

### Logical Example

```bisaya
SUGOD
MUGNA NUMERO a=100, b=200, c=300  
MUGNA TINUOD d="DILI"  
d = (a < b UG c <> 200)  
IPAKITA: d  
KATAPUSAN
```

**Output:**

```
OO
```

## Contributors

<a href="https://github.com/skeletardii/Bisaya-pp/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=skeletardii/Bisaya-pp" />
</a>


## Contact

| Contributor          | GitHub                                                   | Email                           |
|----------------------|----------------------------------------------------------|---------------------------------|
| Felisa Bascug        | [@cusgan](https://www.github.com/cusgan)                 | felisamelaniefay.bascug@cit.edu |
| Roddneil Gemina      | [@RoddneilGemina](https://www.github.com/RoddneilGemina) | roddneil.gemina@cit.edu         |
| Basil Xavier Mendoza | [@skeletardii](https://www.github.com/skeletardii)       | basilxavier.mendoza@cit.edu     |
