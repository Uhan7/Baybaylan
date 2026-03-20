import re
import os

base_dir = os.path.dirname(os.path.abspath(__file__))

def remove_parentheses(text):
    """
    Removes anything inside parentheses including the parentheses.
    Example: 'bata (child)' -> 'bata'
    """
    return re.sub(r"\(.*?\)", "", text)

def remove_punctuation(text):
    """
    Removes punctuation marks.
    """
    return re.sub(r"[^\w\s]", "", text)

def convert_lowercase(text):
    return text.lower()

def split_entries(text):
    """
    Splits entries separated by commas or spaces into individual words.
    """
    words = re.split(r"[,\s]+", text)
    return [w.strip() for w in words if w.strip()]

def remove_invalid_words(word_list):
    """
    Removes words containing disallowed characters.
    """
    return [word for word in word_list if "c" not in word]

def replace_characters(word):
    """
    Conversions
    ñ -> ny
    f -> p
    j -> dy
    qu -> kw
    v -> b
    x -> ks
    z -> s
    """
    word = word.replace("ñ", "ny").replace("Ñ", "Ny")
    word = word.replace("qu", "kw")
    word = word.replace("ch", "ts")
    word = word.replace("f", "p")
    word = word.replace("j", "dy")
    word = word.replace("v", "b")
    word = word.replace("x", "ks")
    word = word.replace("z", "s")
    return word

def remove_duplicates(word_list):
    """
    Removes duplicate entries while keeping order.
    """
    seen = set()
    result = []

    for word in word_list:
        if word not in seen:
            seen.add(word)
            result.append(word)

    return result


def clean_wordlist(input_file, output_file):
    """
    Runs all cleanup steps in order.
    """

    # Read file
    with open(input_file, "r", encoding="utf-8") as f:
        text = f.read()

    # Step 1: remove parentheses
    text = remove_parentheses(text)

    # Step 2: remove punctuations
    text = remove_punctuation(text)

    # Step 3: convert word to lowercase
    text = convert_lowercase(text)

    # Step 4: convert ñ->ny, ch-> ts, f->p, j->dy, qu->kw, v->b, x->ks, z->s
    text = replace_characters(text)

    # Step 5: split entries -> NOW USING LIST
    words = split_entries(text)

    # Step 6: remove words with unused characters (just 'c'... for now)
    words = remove_invalid_words(words)

    # Step 7: remove duplicates
    words = remove_duplicates(words)

    # Write cleaned file
    with open(output_file, "w", encoding="utf-8") as f:
        for word in words:
            f.write(word + "\n")

    print("Cleaning success! Your file is in", str(output_path))

# Get wordlist file name
wordlist_name = input("What is the wordlist filename? (exclude .txt): ")

input_path = os.path.join(base_dir, "..", wordlist_name + ".txt")
output_path = os.path.join(base_dir, "..", wordlist_name + "_clean.txt")

# Run cleanup
clean_wordlist(input_path, output_path)