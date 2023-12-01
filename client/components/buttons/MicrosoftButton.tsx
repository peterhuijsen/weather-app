import React, {MouseEventHandler} from "react";
import GoogleIcon from "@/components/icons/GoogleIcon";
import RightArrowIcon from "@/components/icons/RightArrowIcon";
import MicrosoftIcon from "@/components/icons/MicrosoftIcon";

type MicrosoftButtonProps = {
	onClick: MouseEventHandler<HTMLButtonElement>
}

export default function MicrosoftButton({ onClick } : MicrosoftButtonProps) {
	return (
		<button onClick={onClick} className={"flex flex-row justify-between items-center px-6 py-3 w-full rounded-xl bg-secondary"}>
			<div className={"flex flex-row items-center gap-4 "}>
				<MicrosoftIcon className={"text-white h-4 w-4"} />
				<p className={"font-bold text-md text-white"}>
					Continue with microsoft
				</p>
			</div>
			<RightArrowIcon className={"text-white w-4 h-4"} />
		</button>
	)
}