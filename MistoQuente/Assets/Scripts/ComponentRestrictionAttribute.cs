/*
 * Copyright (c) 2014 - 2022 t_saki@serenegiant.com 
 */

 /*
 Em resumo, esse atributo personalizado é usado para documentar e impor 
 restrições sobre os tipos de componentes Unity que podem ser atribuídos a 
 um campo ou propriedade. Quando o atributo é aplicado a um campo, ele 
 ajuda a comunicar claramente o tipo esperado de componente Unity que deve 
 ser atribuído a esse campo e pode ser usado para verificar essa restrição 
 em tempo de compilação ou em tempo de execução, dependendo de como é 
 implementado no código. 
 */
using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ComponentRestrictionAttribute : PropertyAttribute
{
	public readonly Type type;
	public ComponentRestrictionAttribute(Type type)
	{
		this.type = type;
	}

} // class ComponentRestrictionAttribute

