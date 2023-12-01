import React, {JSX, MouseEventHandler} from "react";

type ButtonStyle = "primary" | "secondary" | "tertiary";
type ButtonType = "submit" | "reset" | "button";

type ButtonProps = {
	onClick?: MouseEventHandler<HTMLButtonElement>;
	type?: ButtonType | undefined;
	style?: ButtonStyle | undefined;
	icon?: JSX.Element | undefined;
	title: string
}

export default function Button ({ title, onClick, type, icon, style = "primary" } : ButtonProps) {
	return style === "primary" ? (
		<button onClick={onClick} type={type} className={"flex flex-row justify-between items-center px-6 py-3 w-full rounded-xl bg-primary"}>
			<p className={"font-bold text-md text-white"}>
				{title}
			</p>
			{icon}
		</button>
	) : (
		<button onClick={onClick} type={type} className={"flex flex-row items-center gap-2 px-6 py-3 w-full rounded-xl bg-tertiary"}>
			{icon}
			<p className={"font-bold text-md text-white"}>
				{title}
			</p>
		</button>
	)
}