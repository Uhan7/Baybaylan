wordlist = []
with open("../AZ_Filipino-wordlist_clean_converted.txt", "r", encoding="utf-8") as f:
	wordlist = [line.strip().lower() for line in f if line.strip()]

with open("../LL_tagalog_word_list_cleaned_converted.txt", "r", encoding="utf-8") as f:
    wordlist.append([line.strip().lower() for line in f if line.strip()])

consonant = ["b", "k", "d", "g", "h", "l", "m", "n", "p", "r", "s", "t", "w", "y"]
vowel = ["a", "e", "i", "o", "u", "-"]

lema = {}



def tokenize(text):
    word = list(text)
    # print(text);
    while word:
        c = word.pop(0)
        if c in consonant:
            if word:
                if word[0] == 'e':
                    word[0] = 'i'
                if word[0] == 'o':
                    word[0] = 'u'
                if word[0] in vowel:
                    c+=word.pop(0)
                if c == "n" and word[0] == "g":
                    c+=word.pop(0)
                    if word and c in vowel:
                        c+=word.pop(0)
            # print(c);

        if not c in lema:
            lema[c] = 1;
        else:
            lema[c] += 1;


# inp = input("test: ")
# tokenize(inp)
# print(lema)
# exit()
for text in wordlist :
    tokenize(text);

total = 0

for word in lema:
    print(word, end=", ")
    total += lema[word]
print(total);

#consonant code
gross = 0
for c in consonant:
    percent = 0;
    nga_percent = 0;
    for token in lema:
        if c == "g" and "ng" in token:
            continue
        if "ng" in token and c == "n":
            nga_percent += lema[token];
        elif c == "n" and c in token:
            percent += lema[token]
        elif c in token:
            percent += lema[token];
    print(f"{c}: {float(int(percent/total*100))/100}") 
    gross += percent/total 
    if nga_percent > 0:
        print(f"nga: {float(int(nga_percent/total*100))/100}") 
        gross += nga_percent/total


print(f"{gross=}")

#diacritic code

for c in vowel:
    percent = 0
    for token in lema:
        if c in token:
            percent += lema[token]
    print(f"{c}: {float(int(percent/total*100))/100}")

print("\n")
for token in lema:
    if token in vowel:
        percent = lema[token]
        print(f"{token}: {float(int(percent/total*100))/100}")


