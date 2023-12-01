import React, {MouseEventHandler} from "react";
import PasskeyIcon from "@/components/icons/PasskeyIcon";
import RightArrowIcon from "@/components/icons/RightArrowIcon";

type PasskeyButtonProps = {
	onClick: MouseEventHandler<HTMLButtonElement>
}

export default function PasskeyButton({ onClick } : PasskeyButtonProps) {
	return (
		<button onClick={onClick} className={"flex flex-row justify-between items-center px-6 py-3 w-full rounded-xl bg-secondary"}>
			<div className={"flex flex-row items-center gap-4 "}>
				<PasskeyIcon className={"text-white h-4 w-4"} />
				<p className={"font-bold text-md text-white"}>
					Continue with passkey
				</p>
			</div>
			<RightArrowIcon className={"text-white w-4 h-4"} />
		</button>
	)
}