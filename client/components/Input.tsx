import {ChangeEventHandler, FocusEventHandler, HTMLInputTypeAttribute} from "react";

type InputProps = {
	type?: HTMLInputTypeAttribute | undefined,
	value?: string | undefined,
	minimumLength?: number | undefined,
	onChange?: ChangeEventHandler<HTMLInputElement> | undefined,
	onBlur?: FocusEventHandler<HTMLInputElement> | undefined,
	name: string,
	title: string,
	placeholder: string,
	disabled?: boolean,
	focus?: boolean,
	required?: boolean
}

export default function Input({ name, title, placeholder, minimumLength, disabled = false, focus = false, required = false, type, value, onChange, onBlur } : InputProps) {
	return (
		<div className={`flex flex-col w-full px-4 py-2 border-2 border-neutral-300 rounded-lg
			focus-within:ring-2 focus-within:ring-primary
			${disabled ? "bg-neutral-200" : ""}`
		}>
			<label htmlFor={title} className={"text-neutral-400 text-sm"}>
				{title}{required && "*"}
			</label>
			<input
				onBlur={onBlur}
				onChange={onChange}
				defaultValue={value}
				minLength={minimumLength}
				autoFocus={focus}
				disabled={disabled}
				className={`text-neutral-900 text-md bg-transparent
					autofill:fill-primary autofill:rounded-md
					placeholder:text-neutral-500
					focus:outline-none
					disabled:text-neutral-500`
				}
				placeholder={placeholder}
				type={type}
				id={name}
				name={name}
			/>
		</div>
	)
}