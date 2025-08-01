using System.Text;

namespace DudNet.Utilities;

internal sealed class IndentedStringBuilder : IIndentedStringBuilder
{
	private readonly char _indentationChar;
	private readonly StringBuilder _stringBuilder;
	private int _indentationLevel;
	private string _indentationString;

	public IndentedStringBuilder(char indentationChar = '\t', int startingIndentation = 0)
	{
		_stringBuilder = new StringBuilder();
		_indentationChar = indentationChar;
		_indentationLevel = startingIndentation;
		_indentationString = GenerateIndentationString();
	}

	private void IncrementIndentation(int amount = 1)
	{
		_indentationLevel += amount;
		_indentationString = GenerateIndentationString();
	}

	private void DecrementIndentation(int amount = 1)
	{
		_indentationLevel = Math.Max(0, _indentationLevel -= amount);
		_indentationString = GenerateIndentationString();
	}

	public IIndentedStringBuilder Append(char character)
	{
		_stringBuilder.Append(character);
		return this;
	}

	public IIndentedStringBuilder AppendLine()
	{
		_stringBuilder.AppendLine();
		return this;
	}

	public IIndentedStringBuilder AppendLine(char character)
	{
		_stringBuilder.AppendLine($"{_indentationString}{character}");
		return this;
	}

	public IIndentedStringBuilder AppendLine(string line)
	{
		_stringBuilder.AppendLine($"{_indentationString}{line}");
		return this;
	}

	public IIndentedStringBuilder BlockWrite(Action<IIndentedStringBuilder> action)
	{
		action(this);
		return this;
	}

	public IIndentedStringBuilder IndentedBlockWrite(Action<IIndentedStringBuilder> action)
	{
		IncrementIndentation();
		action(this);
		DecrementIndentation();
		return this;
	}

	private string GenerateIndentationString()
	{
		return new string(_indentationChar, _indentationLevel);
	}

	public override string ToString()
	{
		return _stringBuilder.ToString();
	}
}