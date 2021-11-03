numbers = [1, 2, 3, 4, 5]
for n in numbers:
    print(n, end=" ")


class anonymousObject(dict):
    __getattr__ = dict.get
    __setattr__ = dict.__setitem__


o = anonymousObject(id=1, name="User One", gdpr=True)

print(anonymousObject)

if anonymousObject.gdpr:
    print("Save user in data base.")
