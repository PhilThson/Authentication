﻿namespace Authentication.Domain.DTOs.Read;

public record ReadUserDto
{
	public int Id { get; init; }
	public string Name { get; init; }
	public string Email { get; init; }
	public bool IsActive { get; set; }
}