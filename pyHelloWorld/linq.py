class SampleObject:

    @property
    def TestProperty(self):
        test = 0
        test = 1
        return test

    def __init___(self):
        # ...
        print("Need some code for linter.")

    def __str__(self):
        return "I'm a sample object."


listSampeObjects = [
    SampleObject(TestProperty=1)
    for o in SampleObject
    if o.TestProperty == 1
]

listSampeObjects.sort(key=lambda t: t.TestProperty)
