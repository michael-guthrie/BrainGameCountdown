// See https://aka.ms/new-console-template for more information
int[]? numbers = null;
int targetNumber = int.MinValue;

Console.WriteLine("NOTE: Performs at O(n!) according to available numbers.");
while (numbers == null)
{
    Console.WriteLine("Enter the available numbers (comma separated):");
    string[]? numbersInput = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries);
    if (numbersInput != null && numbersInput.All(numberInput => int.TryParse(numberInput.Trim(), out int _)))
    {
        numbers = numbersInput.Select(numberInput => int.Parse(numberInput.Trim())).ToArray();
        Console.WriteLine("You entered: " + string.Join(", ", numbers));
        if (numbers.Length > 6)
        {
            Console.WriteLine("WARNING: This may take a while... Continue (y/n)?");
            ConsoleKey input;
            while ((input = Console.ReadKey().Key) is not (ConsoleKey.Y or ConsoleKey.N)) { /* Wait for valid input */ }
            if (input == ConsoleKey.N)
            {
                numbers = null;
            }
            Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine("Invalid input. Please try again.");
    }
}
while (targetNumber == int.MinValue)
{
    Console.WriteLine("Enter the target number:");
    string? numberInput = Console.ReadLine();
    if (int.TryParse(numberInput, out int number))
    {
        targetNumber = number;
        Console.WriteLine("You entered: " + targetNumber);
    }
    else
    {
        Console.WriteLine("Invalid input. Please try again.");
    }
}
Console.WriteLine("Off we go...");
await FindResults(numbers, targetNumber);
Console.WriteLine("Done!");

async Task FindResults(int[]? numbers, int targetNumber)
{
    if (numbers == null)
    {
        return;
    }
    var workingNumbers = numbers.Select(number => new WorkingNumber { Number = number, Source = number.ToString() }).ToArray();
    var results = await DeriveNextWorkingNumbers(targetNumber, workingNumbers);
    foreach (var result in results.Distinct())
    {
        Console.WriteLine($"{result.Number} = {result.Source}");
    }
}
async Task<IEnumerable<WorkingNumber>> DeriveNextWorkingNumbers(int targetNumber, WorkingNumber[] workingNumbers)
{
    // Check up front if we have an answer.
    var result = workingNumbers.Where(wn => wn.Number == targetNumber);
    if (result.Any())
    {
        return result;
    }
    else if (workingNumbers.Length == 1)
    {
        // We have no answer and no more numbers to work with.
        return Enumerable.Empty<WorkingNumber>();
    }
    // Work through the set, grabbing two numbers at a time to derive a composite number,
    // and work on that composite number that with the rest of the set.
    var waitResult = await Task.WhenAll(workingNumbers.Select(async (workingNumber, index1) => 
    {
        var localResult = Enumerable.Empty<WorkingNumber>();
        for (int index2 = 0; index2 < workingNumbers.Length; index2++)
        {
            if (index2 == index1)
            {
                // Same working number. Skip.
                continue;
            }
            var otherWorkingNumber = workingNumbers[index2];
            var nextSet = workingNumbers.Where((wn, index3) => index3 != index1 && index3 != index2).ToArray();
            localResult = localResult.Concat(await DeriveNextWorkingNumbers(targetNumber, nextSet
                .Concat(new[] { new WorkingNumber
                    {
                        Number = workingNumber.Number + otherWorkingNumber.Number,
                        Source = $"({workingNumber.Source} + {otherWorkingNumber.Source})"
                    } }).ToArray()));
            localResult = localResult.Concat(await DeriveNextWorkingNumbers(targetNumber, nextSet
                .Concat(new[] { new WorkingNumber
                    {
                        Number = workingNumber.Number - otherWorkingNumber.Number,
                        Source = $"({workingNumber.Source} - {otherWorkingNumber.Source})"
                    } }).ToArray()));
            localResult = localResult.Concat(await DeriveNextWorkingNumbers(targetNumber, nextSet
                .Concat(new[] { new WorkingNumber
                    {
                        Number = workingNumber.Number * otherWorkingNumber.Number,
                        Source = $"({workingNumber.Source} * {otherWorkingNumber.Source})"
                    } }).ToArray()));

            // Check division only if result is valid and an integer.
            if (otherWorkingNumber.Number != 0 && workingNumber.Number % otherWorkingNumber.Number == 0)
            {
                localResult = localResult.Concat(await DeriveNextWorkingNumbers(targetNumber, nextSet
                    .Concat(new[] { new WorkingNumber
                        {
                            Number = workingNumber.Number / otherWorkingNumber.Number,
                            Source = $"({workingNumber.Source} / {otherWorkingNumber.Source})"
                        } }).ToArray()));
            }

            // Check power only if exponent is valid and small.
            if ((otherWorkingNumber.Number is >= 0 and <= 4) && Math.Abs(workingNumber.Number) < (2048 >> otherWorkingNumber.Number))
            {
                localResult = localResult.Concat(await DeriveNextWorkingNumbers(targetNumber, nextSet
                    .Concat(new[] { new WorkingNumber
                        {
                            Number = (int)Math.Pow(workingNumber.Number, otherWorkingNumber.Number),
                            Source = $"({workingNumber.Source} ^ {otherWorkingNumber.Source})"
                        } }).ToArray()));
            }
        }
        return localResult;
    }));
    return waitResult.SelectMany(r => r);
}
