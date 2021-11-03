def FindNumber(predicate):
    for i in range(100):
        if predicate(i):
            yield i


nums = FindNumber(lambda n: n % 10 == 0)
